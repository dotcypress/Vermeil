namespace Vermeil.Notify
{
    public interface IProgressIndicatorService
    {
        bool IsBusy { get; }
        void ShowIndeterminate(string message, object token);
        void ShowProgress(double progress, string message, object token);
        void Hide(object token);
    }
}