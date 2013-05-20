#region

using System.Windows.Navigation;

#endregion

namespace Vermeil.Navigation
{
    public interface INavigationManager
    {
        NavigationService NavigationService { get; }
        void Navigate(string uri);
        void Navigate(string uri, PageQuery query);
        void GoBack();
        string GetQueryParameter(string key);
        void GoBack<T>(T state) where T : class;
        T GetState<T>();
    }
}
