using System;
using System.Windows.Input;

namespace ESRIJOfflineApp.Commands
{
    class DelegateCommand : ICommand
    {
        private Action<object> _method;
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateCommand"/> class.
        /// </summary>
        public DelegateCommand(Action<object> method)
            : this(method, null)
        {
        }

        private readonly Predicate<object> _canExecute;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateCommand"/> class with parameter.
        /// </summary>
        public DelegateCommand(Action<object> method, Predicate<object> canExecute)
        {
            _method = method;
            _canExecute = canExecute;
        }

        /// <summary>
        /// Sets whether command is enabled or not
        /// </summary>
        public bool CanExecute(object parameter)
        {
            if (_canExecute == null) { return true; }
            return _canExecute(parameter);
        }

        /// <summary>
        /// Execute method for the command
        /// </summary>
        public void Execute(object parameter)
        {
            if (!CanExecute(parameter))
                throw new ArgumentException("Command is not in an executable state");
            _method.Invoke(parameter);
        }

        /// <summary>
        /// Fires if can execute changes
        /// </summary>
        protected virtual void OnCanExecuteChanged(EventArgs e)
        {
            CanExecuteChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Raise can execute changes
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            OnCanExecuteChanged(EventArgs.Empty);
        }
    }
}
