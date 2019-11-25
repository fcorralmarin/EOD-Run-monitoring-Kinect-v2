using System;
using System.Windows.Input;

namespace Common.Utils.Core.Commands
{
    public class RelayCommand : IBaseCommand
    {
        private Action Action;
        public RelayCommand(Action action, Func<bool> canExecute)
        {
            this.Action = action;
            this._CanExecute = canExecute;
        }
        public RelayCommand(Action action)
        {
            this.Action = action;
            this._CanExecute = () => true;
        }
        public override bool CanExecute(object parameter)
        {
            bool result = this._CanExecute.Invoke();
            return result;
        }
        public override void Execute(object parameter)
        {
            this.Action.Invoke();
        }
    }
}

