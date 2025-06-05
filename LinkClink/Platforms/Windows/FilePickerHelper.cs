using Windows.Storage.Pickers;
using Microsoft.Maui.Platform; // ✅ Fix ambiguous Application reference
using Microsoft.UI.Xaml;      // For WinRT window handling

namespace LinkClink.Platforms.Windows;

public static class FilePickerHelper
{
    public static async Task<string?> PickSavePathAsync(string defaultFileName)
    {
        var picker = new FileSavePicker();

        // Fix for CS8602: Ensure Application.Current and Windows[0] are not null before accessing them  
        var application = Microsoft.Maui.Controls.Application.Current;
        if (application?.Windows?.Count > 0)
        {
            var window = application.Windows[0];
            var handler = window.Handler;

            // Fix for CS8600: Ensure handler and PlatformView are not null before accessing them  
            if (handler?.PlatformView is MauiWinUIWindow mauiWinUIWindow)
            {
                var hwnd = mauiWinUIWindow.WindowHandle;
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

                picker.SuggestedStartLocation = PickerLocationId.Downloads;
                picker.DefaultFileExtension = ".mp4";
                picker.SuggestedFileName = defaultFileName;
                picker.FileTypeChoices.Add("MP4 Video", new List<string>() { ".mp4" });

                var file = await picker.PickSaveFileAsync();
                return file?.Path;
            }
        }

        return null; // Return null if Application.Current or Windows[0] is null  
    }
}