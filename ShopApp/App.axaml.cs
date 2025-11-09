using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace QuadroApp
{
    public partial class App : Application
    {
        public IServiceProvider Services { get; private set; } = null!;

        // ✅ parameterloze constructor vereist door Avalonia
        public App()
        {
        }

        // ✅ extra constructor voor DI
        public App(IServiceProvider services)
        {
            Services = services;
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
