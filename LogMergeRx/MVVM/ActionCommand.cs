﻿using System;
using System.Reactive.Linq;
using System.Windows.Input;

namespace LogMergeRx
{
    public class ActionCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public ActionCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute ?? (o => true);
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) =>
            _canExecute(parameter);

        public void Execute(object parameter) =>
            _execute(parameter);

        public void RaiseCanExecuteChanged() =>
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        public void ExecuteOn(IObservable<object> observable) =>
            observable.Where(CanExecute).Subscribe(Execute);

        public void UpdateCanExecuteOn(IObservable<object> observable) =>
            observable.Subscribe(_ => RaiseCanExecuteChanged());
    }
}