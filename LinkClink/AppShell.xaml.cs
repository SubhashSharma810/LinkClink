using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace LinkClink
{
    public partial class AppShell : Shell
    {
        private bool isDark;

        public AppShell()
        {
            InitializeComponent();

            // Load saved preference
            isDark = Preferences.Get("AppTheme", "Light") == "Dark";
            UpdateThemeIcon();
        }

        private void OnThemeToggleClicked(object sender, EventArgs e)
        {
            isDark = !isDark;

            if (isDark)
                App.ApplyTheme(AppTheme.Dark);
            else
                App.ApplyTheme(AppTheme.Light);

            UpdateThemeIcon();
        }

        private void UpdateThemeIcon()
        {
            var icon = isDark ? "☀️" : "🌙";

            foreach (var item in this.ToolbarItems)
            {
                item.Text = icon;
            }
        }
    }
}