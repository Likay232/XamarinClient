﻿using System.Windows.Input;

namespace MauiApp.Commands;

public class RelayCommand(Action<object> execute, Predicate<object>? canExecute = null) : ICommand
{
    public event EventHandler? CanExecuteChanged;
    
    public bool CanExecute(object? parameter) => canExecute!(parameter!);
    
    public void Execute(object? parameter) => execute(parameter!);
    
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}