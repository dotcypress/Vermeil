#region

using System;
using System.Windows.Input;

#endregion

namespace Vermeil.Commands
{
    public class LazyCommand : ICommand
    {
        private ICommand _command;

        public LazyCommand(ICommand command = null)
        {
            _command = command;
        }

        public ICommand Command
        {
            get { return _command; }
            set
            {
                if (_command != null)
                {
                    _command.CanExecuteChanged -= OnCanExecutedChanged;
                }
                _command = value;
                if (_command != null)
                {
                    _command.CanExecuteChanged += OnCanExecutedChanged;
                }
            }
        }

        public bool CanExecute(object parameter)
        {
            return _command != null && _command.CanExecute(parameter);
        }

        public void Execute(object parameter)
        {
            if (_command != null)
            {
                _command.Execute(parameter);
            }
        }

        public event EventHandler CanExecuteChanged;

        public void OnCanExecutedChanged(object sender, EventArgs e)
        {
            if (_command != null && CanExecuteChanged != null)
            {
                CanExecuteChanged(this, e);
            }
        }
    }
}
