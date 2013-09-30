namespace Vermeil.Notify
{
	public interface IProgressIndicatorService
	{
		void ShowIndeterminate(string message, object token);
        void ShowProgress(double progress, string message, object token);
        void Hide(object token);
	    bool IsBusy { get; }
	}
}