using System;
using System.Windows.Input;

namespace AdskTemplateMepTools.Commands
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly bool _canExecute;
        
        public bool CanExecute(object parameter) => _canExecute;

        public void Execute(object parameter) => _execute(parameter);

        public RelayCommand(Action<object> execute, bool canExecute = true)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}