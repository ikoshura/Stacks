// FILE: Stacks/Core.cs

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection; // <-- Tambahkan using ini
using Microsoft.Extensions.Hosting;

namespace Stacks
{
    public class Core : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TrayViewModel _trayViewModel;

        // Jendela akan diinisialisasi nanti saat pertama kali dibutuhkan.
        private FanView? _fanView;
        private SettingsWindow? _settingsWindow;

        // PERBAIKAN: Kita menyuntikkan IServiceProvider, bukan jendela secara langsung.
        public Core(IServiceProvider serviceProvider, TrayViewModel trayViewModel)
        {
            _serviceProvider = serviceProvider;
            _trayViewModel = trayViewModel;

            // Hubungkan command dari ViewModel ke metode di Core service
            _trayViewModel.ToggleFanViewCommand = new RelayCommand(ToggleFanView);
            _trayViewModel.ShowSettingsCommand = new RelayCommand(ShowSettings);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Tidak ada lagi logika jendela di sini, karena jendela belum tentu ada.
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public void ToggleFanView()
        {
            // Buat FanView jika belum ada
            if (_fanView == null)
            {
                _fanView = _serviceProvider.GetRequiredService<FanView>();
                _fanView.ViewDeactivated += () =>
                {
                    _fanView.Hide();
                    _trayViewModel.IsFanViewOpen = false;
                };
            }

            if (_fanView.IsVisible)
            {
                _fanView.Hide();
                // Event deactivated akan memperbarui ViewModel
            }
            else
            {
                var pos = App.GetMousePosition();
                _fanView.ShowAt(pos);
                _trayViewModel.IsFanViewOpen = true;
            }
        }

        public void ShowSettings()
        {
            // Buat SettingsWindow jika belum ada
            if (_settingsWindow == null)
            {
                _settingsWindow = _serviceProvider.GetRequiredService<SettingsWindow>();
                _settingsWindow.Closing += (s, e) =>
                {
                    e.Cancel = true;
                    _settingsWindow.Hide();
                };
            }

            _settingsWindow.Show();
            _settingsWindow.Activate();
        }
    }
}