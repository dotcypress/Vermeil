#region

using System;
using System.Windows.Input;

#endregion

namespace Vermeil.Commands
{
    public class RelayCommand<T> : ICommand
    {
        private readonly Predicate<T> _canExecute;
        private readonly Action<T> _execute;

        public RelayCommand(Action<T> execute)
            : this(execute, x => true)
        {
        }

        public RelayCommand(Action<T> execute, Predicate<T> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute((T) parameter);
        }

        public void Execute(object parameter)
        {
            _execute.Invoke((T) parameter);
        }

        public event EventHandler CanExecuteChanged;

        public void OnCanExecutedChanged()
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, new EventArgs());
            }
        }
    }
}
