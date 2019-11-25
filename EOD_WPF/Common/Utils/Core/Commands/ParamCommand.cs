using System;
using System.Windows.Input;

namespace Common.Utils.Core.Commands
{
    public class ParamCommand : IBaseCommand
    {
        private Action<object> Action;
        public ParamCommand(Action<object> action)
        {
            Action = action;
            _CanExecute = () => true;
        }

        public ParamCommand(Action<object> action, Func<bool> canExecute)
        {
            Action = action;
            _CanExecute = canExecute;
        }

        public override bool CanExecute(object parameter)
        {
            if (this._CanExecute == null)
            {
                return true;
            }
            else
            {
                bool result = this._CanExecute.Invoke();
                return result;
            }
        }

        public override void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                if (parameter != null)
                {
                    Action(parameter);
                }
            }
        }        
    }
}