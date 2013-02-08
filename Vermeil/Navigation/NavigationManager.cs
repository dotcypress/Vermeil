#region

using System;
using System.Net;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;

#endregion

namespace Vermeil.Navigation
{
    internal class NavigationManager : INavigationManager
    {
        private readonly PhoneApplicationFrame _rootFrame;
        private object _backState;
        private readonly object _syncRoot = new object();

        public NavigationManager(PhoneApplicationFrame rootFrame)
        {
            _rootFrame = rootFrame;
        }

        public void Navigate(string uri)
        {
            Navigate(uri, null);
        }

        public void Navigate(string uri, PageQuery query)
        {
            _rootFrame.Navigate(new Uri(uri + PageQuery.BuildQuery(query), UriKind.RelativeOrAbsolute));
        }

        public void GoBack()
        {
            GoBack<object>(null);
        }

        public void GoBack<T>(T state) where T : class
        {
            lock (_syncRoot)
            {
                if (!NavigationService.CanGoBack)
                {
                    return;
                }
                if (state != null)
                {
                    _backState = state;
                }
                NavigationService.StopLoading();
                NavigationService.GoBack();
            }
        }

        public NavigationService NavigationService
        {
            get
            {
                var page = (Page) _rootFrame.Content;
                return page.NavigationService;
            }
        }

        public T GetState<T>()
        {
            return (T) _backState;
        }

        public string GetQueryParameter(string key)
        {
            var page = (Page) _rootFrame.Content;
            string result;
            var exists = page.NavigationContext.QueryString.TryGetValue(key, out result);
            return exists ? HttpUtility.UrlDecode(result) : null;
        }
    }
}