using Microsoft.Maui;
using Microsoft.Maui.Controls;
using System.Diagnostics;

namespace LinkClink
{
    public partial class App : Application
    {
        public static AppTheme CurrentAppTheme { get; private set; } = AppTheme.Light;
        public App()
        {
            InitializeComponent();

            // Defer theme application till Application.Current is fully initialized

            MainPage = new AppShell();

            var savedTheme = Preferences.Get("AppTheme", "Light");

            if (savedTheme =="Dark")
            {
                ApplyTheme(AppTheme.Dark);
                //Application.Current.RequestedThemeChanged += OnAppThemeChanged;
            }
            else
            {
                ApplyTheme(AppTheme.Light);
                Debug.WriteLine("Application.Current is null during App initialization.");
            }
        }

        // Apply app theme by modifying MergedDictionaries
        public static void ApplyTheme(AppTheme theme)
        {
            CurrentAppTheme = theme;
            Preferences.Set("AppTheme", theme.ToString());
            try
            {
                if (Application.Current?.Resources?.MergedDictionaries != null)
                {
                    var mergedDictionaries = Application.Current.Resources.MergedDictionaries;
                    mergedDictionaries.Clear();

                    if (theme == AppTheme.Dark)
                    {
                        mergedDictionaries.Add(new Resources.Styles.DarkTheme());
                        Debug.WriteLine("Dark theme applied.");
                    }
                    else
                    {
                        mergedDictionaries.Add(new Resources.Styles.LightTheme());
                        Debug.WriteLine("Light theme applied.");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ApplyTheme Exception: {ex.Message}");
            }
        }

        // Theme change hook — must be subscribed to
        private void OnAppThemeChanged(object sender, AppThemeChangedEventArgs e)
        {
            Debug.WriteLine($"Theme changed to: {e.RequestedTheme}");
            ApplyTheme(e.RequestedTheme);
        }
    }
}