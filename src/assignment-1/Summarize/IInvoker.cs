namespace assignment_1.Summarize
{
	public interface IInvoker<TRequest>
	{
		Task<string> Invoke(TRequest request, CancellationToken cancellationToken);
	}
}