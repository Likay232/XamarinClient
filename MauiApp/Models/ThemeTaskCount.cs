﻿using System.ComponentModel;

namespace MauiApp.Models;

public class ThemeTaskCount : INotifyPropertyChanged
{
    public int ThemeId { get; set; }
    
    public string ThemeTitle { get; set; }

    private int _taskCount;
    public int TaskCount
    {
        get => _taskCount;
        set
        {
            if (_taskCount != value)
            {
                _taskCount = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TaskCount)));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
