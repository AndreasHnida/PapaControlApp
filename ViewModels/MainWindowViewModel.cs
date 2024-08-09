using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Timers;
using Avalonia.Threading;
using Avalonia.Interactivity;
using ReactiveUI;
using System.Text;
using System.Reactive;
using Avalonia;
using PapaControlApp.Helpers;
using Microsoft.Win32;

namespace PapaControlApp.ViewModels
{
    public partial class MainWindowViewModel : ReactiveObject
    {
        private const string SettingsFilePath = "settings.dat";
        private readonly byte[] _encryptionKey = Encoding.UTF8.GetBytes("qwertzuiopasdfgh");
        private long _lastTickCount;
        private Stopwatch _stopwatch;
        private Timer _timer;
        private long _totalElapsedSeconds;
        private string _elapsedTime;
        private string _remainingTime;
        private string _allowedTime = "1h";
        private bool _isInputEnabled = false;
        private string _toggleButtonText = "Unlock Input";
        
        public string ElapsedTime
        {
            get => _elapsedTime;
            set => this.RaiseAndSetIfChanged(ref _elapsedTime, value);
        }

        public string RemainingTime
        {
            get => _remainingTime;
            set => this.RaiseAndSetIfChanged(ref _remainingTime, value);
        }

        public string AllowedTime
        {
            get => _allowedTime;
            set
            {
                this.RaiseAndSetIfChanged(ref _allowedTime, value);
                SaveSettings();
            }
        }

        public string ToggleButtonText
        {
            get => _toggleButtonText;
            set => this.RaiseAndSetIfChanged(ref _toggleButtonText, value);
        }

        public bool IsInputEnabled
        {
            get => _isInputEnabled;
            set
            {
                this.RaiseAndSetIfChanged(ref _isInputEnabled, value);
                ToggleButtonText = value ? "Lock Input" : "Unlock Input";
            }
        }

        public MainWindowViewModel()
        {
            
            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
            LoadSettings();
            _stopwatch = new Stopwatch();

            _lastTickCount = Environment.TickCount64;

            _timer = new Timer(1000);
            _timer.Elapsed += OnTimerElapsed;
            StartUsageTimeRecording();
        }

        public void StartUsageTimeRecording()
        {
            _timer.Start();
            _stopwatch.Start();
        }

        public void StopUsageTimeRecording()
        {   
            SaveSettings();
            _timer.Stop();
            _stopwatch.Stop();
        }

        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case PowerModes.Resume:
                    Dlog.Log("System resumed from sleep/hibernation.");
                    // Handle system resume (e.g., restart timer, refresh data)
                    StartUsageTimeRecording();
                    break;
                case PowerModes.Suspend:
                    Dlog.Log("System entering sleep/hibernation.");
                    // Handle system suspend (e.g., pause timer, save state)
                    StopUsageTimeRecording();
                    break;
                // Add cases for other PowerModes as needed
            }
        }
        
        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _totalElapsedSeconds += (long)_stopwatch.Elapsed.TotalSeconds;
            _stopwatch.Restart();
            // Dlog.Log("LastTickCount: " + _lastTickCount);
            if (TryParseHumanReadableTime(_allowedTime, out TimeSpan allowedTimeSpan))
            {
                TimeSpan elapsedTimeSpan = TimeSpan.FromSeconds(_totalElapsedSeconds);
                TimeSpan remainingTimeSpan = allowedTimeSpan - elapsedTimeSpan;

                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    ElapsedTime = FormatTimeSpan(elapsedTimeSpan);
                    RemainingTime = FormatTimeSpan(remainingTimeSpan);
                });

                if (remainingTimeSpan <= TimeSpan.Zero)
                {
                    Debug.WriteLine("Time's up!");
                }
            }
            else
            {
                Debug.WriteLine("Invalid _allowedTime format.");
            }
        }

        private static string FormatTimeSpan(TimeSpan timeSpan)
        {
            return $"{timeSpan.Hours}h {timeSpan.Minutes}m {timeSpan.Seconds}s";
        }

        private static bool TryParseHumanReadableTime(string input, out TimeSpan timeSpan)
        {
            timeSpan = TimeSpan.Zero;
            var regex = new Regex(@"((?<h>\d+)h)?\s*((?<m>\d+)m)?\s*((?<s>\d+)s)?");
            var match = regex.Match(input);
            if (!match.Success) return false;

            int hours = 0, minutes = 0, seconds = 0;
            if (match.Groups["h"].Success) hours = int.Parse(match.Groups["h"].Value);
            if (match.Groups["m"].Success) minutes = int.Parse(match.Groups["m"].Value);
            if (match.Groups["s"].Success) seconds = int.Parse(match.Groups["s"].Value);

            timeSpan = new TimeSpan(hours, minutes, seconds);
            return true;
        }

        public void ResetTimerCommand()
        {
            _totalElapsedSeconds = 0;
        }

        public void QuitCommand()
        {
            Dlog.Log("Quitting...");
        }

        public void SaveSettings()
        {
            try
            {
                var data = $"{_allowedTime}|{_totalElapsedSeconds}";
                var encryptedData = EncryptString(data, _encryptionKey);
                Dlog.Log("Saving settings...");
                File.WriteAllBytes(SettingsFilePath, encryptedData);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to save settings: {ex.Message}");
            }
        }

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    var encryptedData = File.ReadAllBytes(SettingsFilePath);
                    var data = DecryptString(encryptedData, _encryptionKey);
                    var parts = data.Split('|');
                    if (parts.Length >= 2)
                    {
                        _allowedTime = parts[0];
                        _totalElapsedSeconds = long.Parse(parts[1]);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load settings: {ex.Message}");
            }
        }

        private byte[] EncryptString(string plainText, byte[] key)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.GenerateIV();
                var iv = aes.IV;

                using (var encryptor = aes.CreateEncryptor(aes.Key, iv))
                using (var ms = new MemoryStream())
                {
                    ms.Write(iv, 0, iv.Length);
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }

                    return ms.ToArray();
                }
            }
        }


        private string DecryptString(byte[] cipherText, byte[] key)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = key;
                var iv = new byte[aes.BlockSize / 8];
                Array.Copy(cipherText, iv, iv.Length);

                using (var decryptor = aes.CreateDecryptor(aes.Key, iv))
                using (var ms = new MemoryStream(cipherText, iv.Length, cipherText.Length - iv.Length))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}