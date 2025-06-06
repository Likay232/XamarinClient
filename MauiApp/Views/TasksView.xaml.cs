﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MauiApp.Models;
using MauiApp.Services;
using MauiApp.ViewModels;

namespace MauiApp.Views;

[QueryProperty(nameof(ThemeId), "themeId")]
public partial class TasksView : ContentPage
{
    public int ThemeId
    {
        get => ((TasksViewModel)BindingContext).ThemeId;
        set
        {
            ((TasksViewModel)BindingContext).ThemeId = value;
            ((TasksViewModel)BindingContext).LoadTasksAsync();
        }
    }

    public TasksView(TasksViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnTaskTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is TaskForTest taskForTest)
        {
            await Shell.Current.GoToAsync($"{nameof(TaskView)}?taskId={taskForTest.Id}");
        }
    }
}
