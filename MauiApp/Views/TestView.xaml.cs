﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MauiApp.ViewModels;

namespace MauiApp.Views;

public partial class TestView : ContentPage
{
    public TestView(TestViewModel viewModel)
    {
        InitializeComponent();
        
        BindingContext = viewModel;
    }

    private void Entry_Completed(object sender, EventArgs e)
    {
        if (BindingContext is TestViewModel vm && sender is Entry entry)
        {
            vm.SaveAnswerCommand.Execute(entry.Text);
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        if (BindingContext is TestViewModel vm)
            vm.LoadTestAsync();
    }
}