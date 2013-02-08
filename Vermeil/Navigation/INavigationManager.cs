#region

using System.Windows.Navigation;

#endregion

namespace Vermeil.Navigation
{
    public interface INavigationManager
    {
        void Navigate(string uri);
        void Navigate(string uri, PageQuery query);
        void GoBack();
        NavigationService NavigationService { get; }
        string GetQueryParameter(string key);
        void GoBack<T>(T state) where T : class;
        T GetState<T>();
    }
}