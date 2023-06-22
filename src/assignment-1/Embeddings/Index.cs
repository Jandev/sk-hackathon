using Azure.AI.OpenAI;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Reflection;

namespace assignment_1.Embeddings
{
	public class Index
	{
		private const string vectorSearchConfigName = "hackathon-vector-config";
		private const string semanticSearchConfigName = "hackathon-semantic-config";

		private readonly OpenAIClient openAIClient;
		private readonly SearchIndexClient indexClient;
		private readonly Settings.CognitiveSearch cognitiveSearchSettings;
		private readonly Settings.OpenAi openAiSettings;

		public Index(
			OpenAIClient openAIClient,
			SearchIndexClient indexClient,
			IOptions<Settings.CognitiveSearch> cognitiveSearchOptions,
			IOptions<Settings.OpenAi> openAiOptions)
		{
			this.openAIClient = openAIClient;
			this.indexClient = indexClient;
			this.cognitiveSearchSettings = cognitiveSearchOptions.Value;
			this.openAiSettings = openAiOptions.Value;
		}

		public async Task Build()
		{
			var index = CreateIndex();
			indexClient.CreateOrUpdateIndex(index);

			var contentSearchCollection = await GetContentEmbeddingsDocuments();

			var searchClient = this.indexClient.GetSearchClient(cognitiveSearchSettings.VectorIndexName);
			await searchClient.IndexDocumentsAsync(IndexDocumentsBatch.Upload(contentSearchCollection));
		}

		private SearchIndex CreateIndex()
		{
			SearchIndex searchIndex = new(cognitiveSearchSettings.VectorIndexName)
			{
				VectorSearch = new()
				{
					AlgorithmConfigurations =
					{
						new HnswVectorSearchAlgorithmConfiguration(vectorSearchConfigName)
					}
				},
				SemanticSettings = new()
				{
					Configurations =
					{
					   new SemanticConfiguration(semanticSearchConfigName, new()
					   {
						   TitleField = new(){ FieldName = "query" },
						   ContentFields =
						   {
							   new() { FieldName = "content" }
						   }
					   })
					}
				},
				Fields =
				{
					new SimpleField("id", SearchFieldDataType.String) { IsKey = true, IsFilterable = true, IsSortable = true, IsFacetable = true },
					new SearchableField("query") { IsFilterable = true, IsSortable = true },
					new SearchableField("content") { IsFilterable = true },
					new SearchField("contentVector", SearchFieldDataType.Collection(SearchFieldDataType.Single))
					{
						IsSearchable = true,
						VectorSearchDimensions = 1536,
						VectorSearchConfiguration = vectorSearchConfigName
					}
				}
			};

			return searchIndex;
		}

		private async Task<List<ContentSearchDocument>> GetContentEmbeddingsDocuments()
		{
			var loadedContentCollection = GetContentData();

			var documents = new List<ContentSearchDocument>();
			foreach (var loadedContent in loadedContentCollection)
			{
				var calculatedEmbeddings = await GenerateEmbeddings(loadedContent.Query);

				documents.Add(
					new ContentSearchDocument
					{
						contentVector = calculatedEmbeddings.ToArray(),
						Id = loadedContent.Id,
						Query = loadedContent.Query,
						Content = loadedContent.Content
					});
			}

			return documents;
		}

		private IEnumerable<ContentDocument> GetContentData()
		{
			var assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			if (assemblyDirectory == null)
			{
				throw new ApplicationException("Could not get assembly directory.");
			}

			var path = Path.Combine(
				assemblyDirectory,
				"Embeddings",
				"content.json");

			using StreamReader reader = new(path);
			var json = reader.ReadToEnd();
			var jsonData = JsonConvert.DeserializeObject<IEnumerable<ContentDocument>>(json);
			if (jsonData == null)
			{
				throw new ArgumentException("File containing content has an unexpected format.");
			}
			return jsonData;
		}

		private async Task<IReadOnlyList<float>> GenerateEmbeddings(
			string text)
		{
			var response = await openAIClient.GetEmbeddingsAsync(
				this.openAiSettings.EmbeddingsDeploymentId,
				new EmbeddingsOptions(text));
			return response.Value.Data[0].Embedding;
		}

		public class ContentDocument
		{
			public string Id { get; set; } = string.Empty;
			
			public string Query { get; set; } = string.Empty;

			public string Content { get; set; } = string.Empty;
		}

		public class ContentSearchDocument
		{
			[JsonProperty("id")]
			public string Id { get; set; } = string.Empty;

			[JsonProperty("query")]
			public string Query { get; set; } = string.Empty;

			[JsonProperty("content")]
			public string Content { get; set; } = string.Empty;

			[JsonProperty("contentVector")]
			public float[] contentVector { get; set; } = Array.Empty<float>();
		}
	}
}
