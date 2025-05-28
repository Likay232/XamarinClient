﻿using System.Windows.Input;
using MauiApp.Commands;
using MauiApp.Models;
using MauiApp.Services;

namespace MauiApp.ViewModels;

public class RegisterViewModel : ViewModelBase<RegisterModel>
{
    private string? _errorMessage;
    public string? ErrorMessage
    {
        get => _errorMessage;
        set
        {
            if (_errorMessage != value)
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }
    }

    public string? Username
    {
        get => Model.Username;
        set
        {
            if (Username != value)
            {
                Model.Username = value ?? "";
                OnPropertyChanged();
                ((RelayCommand)RegisterCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public string? Password
    {
        get => Model.Password;
        set
        {
            if (Password != value)
            {
                Model.Password = value ?? "";
                OnPropertyChanged();
                ((RelayCommand)RegisterCommand).RaiseCanExecuteChanged();
            }
        }
    }
    
    public string? ConfirmPassword
    {
        get => Model.ConfirmPassword;
        set
        {
            if (ConfirmPassword != value)
            {
                Model.ConfirmPassword = value ?? "";
                OnPropertyChanged();
                ((RelayCommand)RegisterCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public string? FirstName
    {
        get => Model.FirstName;
        set
        {
            if (FirstName != value)
            {
                Model.FirstName = value ?? "";
                OnPropertyChanged();
                ((RelayCommand)RegisterCommand).RaiseCanExecuteChanged();
            }
        }
    }
    
    public string? LastName
    {
        get => Model.LastName;
        set
        {
            if (LastName != value)
            {
                Model.LastName = value ?? "";
                OnPropertyChanged();
                ((RelayCommand)RegisterCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public ICommand RegisterCommand { get; set; }

    public RegisterViewModel(ApiService service)
    {
        _apiService = service;

        Model = new RegisterModel();

        RegisterCommand = new RelayCommand(ExecuteRegister, CanExecuteRegister);
    }
    
    private bool CanExecuteRegister(object obj)
    {
        if (obj is not RegisterModel regModel) return false;
        return !string.IsNullOrWhiteSpace(regModel.Username)
               && !string.IsNullOrWhiteSpace(regModel.Password)
               && !string.IsNullOrWhiteSpace(regModel.ConfirmPassword)
               && !string.IsNullOrWhiteSpace(regModel.FirstName)
               && !string.IsNullOrWhiteSpace(regModel.LastName);
    }

    private async void ExecuteRegister(object obj)
    {
        ErrorMessage = null;
        
        if (obj is not RegisterModel regModel) return;
            
        if (string.IsNullOrWhiteSpace(regModel.Username) || string.IsNullOrWhiteSpace(regModel.Password))
        {
            return;
        }

        if (await RegisterUser(regModel))
        {
            Model.FirstName = "";
            Model.LastName = "";
            Model.Username = "";
            Model.Password = "";
            Model.ConfirmPassword = "";
            
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (Application.Current == null) return;
                if (Application.Current.MainPage == null) return;
                
                await Application.Current.MainPage.Navigation.PopModalAsync();
            });
        }
        else
        {
            Model.Username = "";
            ErrorMessage = "Имя пользователя занято.";
        }
    }

    private async Task<bool> RegisterUser(RegisterModel regModel)
    {
        if (regModel.Password != regModel.ConfirmPassword)
        {
            ErrorMessage = "Пароли не совпадают.";
            Model.Password = "";
            Model.ConfirmPassword = "";
            return false;
        }

        return await _apiService.RegisterUser(regModel);
    }

}