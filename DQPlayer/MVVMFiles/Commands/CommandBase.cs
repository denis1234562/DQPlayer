using System;
using System.Windows.Input;

namespace DQPlayer.MVVMFiles.Commands
{
    public class CommandBase<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public CommandBase(Action<T> execute, Predicate<T> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute != null)
            {
                return _canExecute((T)parameter);
            }
            return true;
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }
    }
}
