using Android.App;
using Android.Content;
using Android.OS;
using System.IO;

namespace LinkClink.Platforms.Android
{
    public static class FilePickerHelper
    {
        public static string PickSavePath(string suggestedFileName = "video")
        {
            // Corrected the context retrieval to use the proper namespace and method  
            var context = Application.Context;
            var downloadsPath = context.GetExternalFilesDir(Environment.DirectoryDownloads)?.AbsolutePath;

            if (downloadsPath == null)
                downloadsPath = context.FilesDir?.AbsolutePath ?? "/storage/emulated/0/Download";

            var fullPath = Path.Combine(downloadsPath, suggestedFileName + ".mp4");
            return fullPath;
        }
    }
}   