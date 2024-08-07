using System.Diagnostics;

namespace PapaControlApp.Helpers
{
    public static class Dlog
    {
        public static bool IsEnabled { get; set; } = true;

        public static void Log(string message)
        {
            if (IsEnabled)
                Debug.WriteLine(message);
        }
    }
}