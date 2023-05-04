namespace assignment_1.Summarize
{
	internal class Website : IInvoker<WebsiteRequest>
	{
		public Task<string> Invoke(WebsiteRequest request)
		{
			throw new NotImplementedException();
		}
	}

	public record WebsiteRequest(Uri url);
}
