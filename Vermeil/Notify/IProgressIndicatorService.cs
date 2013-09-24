namespace Vermeil.Notify
{
	public interface IProgressIndicatorService
	{
		void ShowIndeterminate(string message, string token);
		void ShowProgress(double progress, string message, string token);
		void Hide(string token);
	}
}