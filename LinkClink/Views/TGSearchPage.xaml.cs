using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;

namespace LinkClink.Views;

public partial class TGSearchPage : ContentPage
{
    private readonly HttpClient _httpClient = new();

    public TGSearchPage()
    {
        InitializeComponent();
    }

    private async void OnSearchClicked(object sender, EventArgs e)
    {
        string? query = SearchBox.Text?.Trim();

        if (string.IsNullOrWhiteSpace(query))
        {
            await DisplayAlert("Input Required", "Please enter a query to search.", "OK");
            return;
        }

        try
        {
            ResultsContainer.Children.Clear();

                #if ANDROID
                            var baseUrl = "http://10.0.2.2:5000";
                #else
                            var baseUrl = "http://localhost:5000";
                #endif

            var url = $"{baseUrl}/api/tgsearch?q={Uri.EscapeDataString(query)}";
            var response = await _httpClient.GetStringAsync(url);
            var data = JArray.Parse(response);

            foreach (var channel in data)
            {
                string? name = channel["name"]?.ToString();
                string? username = channel["username"]?.ToString();
                string? profilePic = channel["avatar"]?.ToString();
                string? link = channel["link"]?.ToString();

                if (name == null || username == null || profilePic == null || link == null)
                {
                    continue; // Skip this channel if any required field is null
                }

                // Fix for CS8602: Ensure Application.Current and Resources are not null
                if (Application.Current?.Resources != null && Application.Current.Resources.TryGetValue("FrameColor", out var frameColorObj) && frameColorObj is Color frameColor)
                {
                    var card = new Frame
                    {
                        CornerRadius = 16,
                        Padding = 12,
                        BackgroundColor = frameColor,
                        HasShadow = true,
                        Content = new HorizontalStackLayout
                        {
                            Spacing = 12,
                            Children =
                            {
                                new Image
                                {
                                    Source = profilePic,
                                    WidthRequest = 48,
                                    HeightRequest = 48,
                                    Aspect = Aspect.AspectFill,
                                    Clip = new EllipseGeometry { Center = new Point(24, 24), RadiusX = 24, RadiusY = 24 }
                                },
                                new VerticalStackLayout
                                {
                                    Spacing = 4,
                                    Children =
                                    {
                                        new Label
                                        {
                                            Text = name,
                                            FontAttributes = FontAttributes.Bold,
                                            TextColor = Application.Current.Resources["TextColor"] as Color ?? Colors.Black
                                        },
                                        new Label
                                        {
                                            Text = "@" + username,
                                            FontSize = 12,
                                            TextColor = Colors.Gray
                                        },
                                        new Label
                                        {
                                            Text = link,
                                            FontSize = 12,
                                            TextColor = Colors.Blue
                                        }
                                    }
                                }
                            }
                        }
                    };

                    ResultsContainer.Children.Add(card);
                }
                else
                {
                    await DisplayAlert("Error", "FrameColor resource is not defined or Application.Current is null.", "OK");
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "Could not fetch results.\n" + ex.Message, "OK");
        }
    }
}