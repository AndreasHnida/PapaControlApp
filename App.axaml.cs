using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using AvaloniaDialogs.Views;
using Avalonia.Controls.Notifications;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using PapaControlApp.ViewModels;
using PapaControlApp.Views;
using System;

namespace PapaControlApp
{
    public partial class App : Application
    {


        private Window _mainWindow;
        private TrayIcon trayIcon;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            this.DataContext = this;

        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Line below is needed to remove Avalonia data validation.
                // Without this line you will get duplicate validations from both Avalonia and CT
                BindingPlugins.DataValidators.RemoveAt(0);
                _mainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel()
                };
                trayIcon = new TrayIcon();
                trayIcon.Clicked += TrayIcon_Clicked;
                desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                desktop.MainWindow = _mainWindow;
                //desktop.MainWindow = _mainWindow;
                desktop.Exit += (sender, e) => trayIcon.Dispose();
            }

            base.OnFrameworkInitializationCompleted();

        }
        private void Quit_Clicked(object? sender, System.EventArgs e)
        {
            // exit the program 
            Environment.Exit(0);
        }
        private void TrayIcon_Clicked(object? sender, EventArgs e)
        {
            if (_mainWindow != null)
            {
                _mainWindow.Show();
                _mainWindow.Activate();
            }
        }
    }
}