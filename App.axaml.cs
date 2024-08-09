using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using PapaControlApp.ViewModels;
using PapaControlApp.Views;
using System;
using Avalonia.Threading;
using PapaControlApp.Helpers;


namespace PapaControlApp
{
    public partial class App : Application
    {


        private Window _mainWindow;
        private TrayIcon _trayIcon;
        private DispatcherTimer _autosaveTimer;
        private MainWindowViewModel _mainWindowViewModel;
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
                _trayIcon = new TrayIcon();
                _trayIcon.Clicked += TrayIcon_Clicked;
                desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                desktop.MainWindow = _mainWindow;
                _mainWindowViewModel = (MainWindowViewModel)_mainWindow.DataContext;
                //desktop.MainWindow = _mainWindow;
                desktop.Exit += (sender, e) => _trayIcon.Dispose();
                InitAutosave();
            }

            base.OnFrameworkInitializationCompleted();

        }

        private void InitAutosave()
        {
           _autosaveTimer = new DispatcherTimer();
           _autosaveTimer.Interval = TimeSpan.FromSeconds(60);
           _autosaveTimer.Tick += (sender, e) => AutosaveTimer_Tick (sender, e);
           _autosaveTimer.Start();
        }

        private void AutosaveTimer_Tick(object? sender, System.EventArgs e)
        {
            Dlog.Log("AutosaveTimer_Tick");
            _mainWindowViewModel.SaveSettings();
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