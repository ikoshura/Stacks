// FILE: Stacks/TrayViewModel.cs

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Hosting;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace Stacks;

// ObservableObject menangani INotifyPropertyChanged secara otomatis.
public partial class TrayViewModel : ObservableObject
{
    private readonly IHostApplicationLifetime _lifetime;

    // Properti yang akan di-bind ke UI.
    [ObservableProperty]
    private string _iconSource = "Icons/folder.ico";

    [ObservableProperty]
    private string _toolTipText = "Click to open Stacks";

    private bool _isFanViewOpen;
    public bool IsFanViewOpen
    {
        get => _isFanViewOpen;
        set
        {
            // Saat status berubah, perbarui ikon dan tooltip.
            if (SetProperty(ref _isFanViewOpen, value))
            {
                IconSource = value ? "Icons/close.ico" : "Icons/folder.ico";
                ToolTipText = value ? "Click to close Stacks" : "Click to open Stacks";
            }
        }
    }

    // Commands yang akan di-bind ke UI.
    public ICommand ToggleFanViewCommand { get; set; } = null!;
    public ICommand ShowSettingsCommand { get; set; } = null!;

    [RelayCommand]
    private void ExitApplication()
    {
        _lifetime.StopApplication();
    }

    public TrayViewModel(IHostApplicationLifetime lifetime)
    {
        _lifetime = lifetime;
    }
}



