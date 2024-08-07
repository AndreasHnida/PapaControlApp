using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Styling;
using AvaloniaDialogs.Views;
using PapaControlApp.ViewModels;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace PapaControlApp.Views;

public partial class MainWindow : Window
{
    const string CUSTOM_STYLE_KEY = "CustomStyle";

    public MainWindow()
    {
        InitializeComponent();
        this.Closing += MainWindow_Closing;
    }
    public MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext;

    private void MainWindow_Closing(object? sender, WindowClosingEventArgs e)
    {
        e.Cancel = true;
        this.Hide();
    }

    private async void DialogButton_Click(object? sender, RoutedEventArgs args)
    {
        SingleActionDialog dialog = new()
        {
            Message = "Hello from C# code! Do you want to see a snackbar?",
            ButtonText = "Click me!"
        };
        if ((await dialog.ShowAsync()).HasValue)
        {
            Snackbar.Show("I'm a snackbar!", TimeSpan.FromSeconds(2), "I Know");
        }
    }

    private void ToggleInput(object? sender, RoutedEventArgs args)
    {
        var pinPanel = this.FindControl<Grid>("PinEntryPanel");
        pinPanel.Margin = pinPanel.Margin.Left == 0 ? new Thickness(-250, 0, 0, 0) : new Thickness(0);
    }

    private void SubmitPin(object? sender, RoutedEventArgs args)
    {
        var pinTextBox = this.FindControl<TextBox>("PinTextBox");
        if (pinTextBox.Text == "1212") // Replace with your desired PIN
        {
            ViewModel.IsInputEnabled = !ViewModel.IsInputEnabled;
            var pinPanel = this.FindControl<Grid>("PinEntryPanel");
            pinPanel.Margin = pinPanel.Margin.Left == -250 ? new Thickness(0, 0, 0, 0) : new Thickness(-250,0,0,0);
        }
        else
        {
            Snackbar.Show("Incorrect PIN. Access denied.", TimeSpan.FromSeconds(2), "OK");
        }
        pinTextBox.Text = "";
    }
}