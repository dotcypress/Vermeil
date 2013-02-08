#region

using System;
using System.Windows.Input;
using Vermeil.Navigation;

#endregion

namespace Vermeil.Commands
{
    public class NavigationCommand : ICommand
    {
        private readonly string _uri;

        public NavigationCommand(string uri)
        {
            _uri = uri;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            var bootstrapper = Bootstrapper.Current;
            if (bootstrapper != null)
            {
                bootstrapper.Container.Resolve<INavigationManager>().Navigate(_uri, parameter as PageQuery);
            }
        }

        public void OnCanExecutedChanged()
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, new EventArgs());
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}