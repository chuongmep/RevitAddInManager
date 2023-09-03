using System;
using System.Windows.Input;

namespace RevitElementBipChecker.Model
{
    /// <summary>
    /// A general relay command that takes its parameter as an object
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Predicate<object> m_canExecute;
        private readonly Action<object> m_execute;
        private readonly Action _act;

        public RelayCommand(Action act)
        {
            _act = act;
        }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("Execute");
            }
            m_execute = execute;
            m_canExecute = canExecute;
        }

        // Evaluate the command if it is valid to execute
        public bool CanExecute(object parameter = null)
        {
            if (parameter == null || m_canExecute == null) return true;
            else return m_canExecute(parameter);
        }
        // Main execute method
        public void Execute(object parameter = null)
        {
            if (_act != null) _act();
            else m_execute(parameter);
        }

        // In WPF CommandManager is a pre-defined class that take charge of observing the user interface 
        // and calls the CanExecute method when it deems it necessary
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}

