// FILE: Stacks/Program.cs
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Stacks
{
    internal class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddHostedService<Core>();
                    services.AddSingleton<FanView>();
                    services.AddSingleton<SettingsWindow>();
                    services.AddSingleton<TrayViewModel>();
                    services.AddSingleton<MainContextMenu>();
                    services.AddSingleton<Tray>();
                    services.AddSingleton<App>();
                })
                .Build();

            var app = host.Services.GetRequiredService<App>();
            app.Run();
        }
    }
}