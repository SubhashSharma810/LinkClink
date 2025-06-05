using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using System.Diagnostics;

#if WINDOWS
using LinkClink.Platforms.Windows;
#elif ANDROID
using LinkClink.Platforms.Android;
#endif

namespace LinkClink.Views;

public partial class LinkClinkPage : ContentPage
{
    private readonly HttpClient http = new();
    private string selectedFormatId = "";
    private string downloadUrl = "";

    public LinkClinkPage()
    {
        InitializeComponent();
    }

    private async void OnPreviewClicked(object sender, EventArgs e)
    {
        string? url = LinkEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(url))
        {
            await DisplayAlert("Error", "Please enter a valid link", "OK");
            return;
        }

        try
        {
            var body = new StringContent(JsonConvert.SerializeObject(new { url }), Encoding.UTF8, "application/json");

#if ANDROID
            string backendUrl = "http://10.0.2.2:5000"; // Android Emulator
#else
            string backendUrl = "http://localhost:5000"; // Windows / iOS / Mac
#endif

            var response = await http.PostAsync($"{backendUrl}/api/formats", body);

            if (!response.IsSuccessStatusCode)
            {
                await DisplayAlert("Error", "Failed to fetch formats", "OK");
                return;
            }

            var result = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<YouTubeFormatResult>(result);

            if (json?.Formats == null || json.Formats.Count == 0)
            {
                await DisplayAlert("Error", "No formats available", "OK");
                return;
            }

            downloadUrl = url;
            VideoTitleLabel.Text = json?.Title ?? "Untitled";
            ThumbnailImage.Source = json?.Thumbnail ?? "";
            FormatPicker.Items.Clear();

            foreach (var f in json.Formats)
            {
                string label = $"{f.Resolution ?? "Audio"} - {f.Ext} - {(f.HasAudio ? "🎵" : "")}{(f.HasVideo ? "🎥" : "")}";
                FormatPicker.Items.Add(label);
            }

            FormatPicker.SelectedIndexChanged += (s, args) =>
            {
                if (FormatPicker.SelectedIndex >= 0 && FormatPicker.SelectedIndex < json.Formats.Count)
                    selectedFormatId = json.Formats[FormatPicker.SelectedIndex]?.FormatId ?? string.Empty;
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            await DisplayAlert("Error", "Unexpected error", "OK");
        }
    }

    private async void OnDownloadClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(downloadUrl) || string.IsNullOrWhiteSpace(selectedFormatId))
        {
            await DisplayAlert("Error", "Please preview and select a format first", "OK");
            return;
        }

        try
        {
            var body = new StringContent(JsonConvert.SerializeObject(new
            {
                url = downloadUrl,
                format_id = selectedFormatId
            }), Encoding.UTF8, "application/json");

#if ANDROID
            string backendUrl = "http://10.0.2.2:5000";
#else
            string backendUrl = "http://localhost:5000";
#endif

            var response = await http.PostAsync($"{backendUrl}/api/download", body);

            if (!response.IsSuccessStatusCode)
            {
                await DisplayAlert("Error", "Download failed", "OK");
                return;
            }

            var fileBytes = await response.Content.ReadAsByteArrayAsync();
            string safeTitle = VideoTitleLabel.Text ?? "download";
            string fileName = $"{safeTitle}.mp4";
            string? finalPath = null;

#if WINDOWS
            finalPath = await FilePickerHelper.PickSavePathAsync(fileName);
#elif ANDROID
            finalPath = FilePickerHelper.PickSavePath(fileName);
#endif

            if (!string.IsNullOrWhiteSpace(finalPath))
            {
                File.WriteAllBytes(finalPath, fileBytes);
                await DisplayAlert("Success", $"File saved to: {finalPath}", "OK");
            }
            else
            {
                await DisplayAlert("Cancelled", "Save cancelled by user.", "OK");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            await DisplayAlert("Error", "Unexpected error during download", "OK");
        }
    }

    public class YouTubeFormatResult
    {
        [JsonProperty("title")]
        public string? Title { get; set; }

        [JsonProperty("thumbnail")]
        public string? Thumbnail { get; set; }

        [JsonProperty("formats")]
        public List<Format> Formats { get; set; } = [];
    }

    public class Format
    {
        [JsonProperty("format_id")]
        public string? FormatId { get; set; }

        [JsonProperty("resolution")]
        public string? Resolution { get; set; }

        [JsonProperty("ext")]
        public string Ext { get; set; } = string.Empty;

        [JsonProperty("has_audio")]
        public bool HasAudio { get; set; }

        [JsonProperty("has_video")]
        public bool HasVideo { get; set; }
    }
}