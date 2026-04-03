using Turntable.Models;
using Turntable.Services;
using Firebase.Storage;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Turntable.Views.Seller;

public partial class AddListingPage : ContentPage
{
    private string _selectedFulfillment = "Shipping";
    private FileResult _selectedImage;
    private ListingModel _editingListing = null;
    private string _apiImageUrl = "";

    public AddListingPage()
    {
        InitializeComponent();
    }

    public AddListingPage(ListingModel listingToEdit)
    {
        InitializeComponent();
        _editingListing = listingToEdit;

        ArtistEntry.Text = _editingListing.Artist;
        AlbumEntry.Text = _editingListing.AlbumName;
        YearEntry.Text = _editingListing.ReleaseYear.ToString();
        DurationEntry.Text = _editingListing.Duration;
        PriceEntry.Text = _editingListing.Price.ToString();
        GenrePicker.SelectedItem = _editingListing.Genre;
        ConditionPicker.SelectedItem = _editingListing.Condition;

        if (_editingListing.Tracklist != null)
        {
            TracklistEditor.Text = string.Join("\n", _editingListing.Tracklist);
        }

        var publishBtn = this.FindByName<Button>("PublishBtn");
        if (publishBtn != null) publishBtn.Text = "UPDATE LISTING";
    }

    private async void OnAutoFillClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ArtistEntry.Text) || string.IsNullOrWhiteSpace(AlbumEntry.Text))
        {
            await DisplayAlert("Missing Info", "Please enter the Artist and Album Title first.", "OK");
            return;
        }

        Button btn = (Button)sender;
        btn.Text = "FETCHING DATA...";
        btn.IsEnabled = false;

        try
        {
            string artist = Uri.EscapeDataString(ArtistEntry.Text.Trim());
            string album = Uri.EscapeDataString(AlbumEntry.Text.Trim());
            string url = $"http://ws.audioscrobbler.com/2.0/?method=album.getinfo&api_key=066831ef9b28a8c1db2a5c7a8dffbc30&artist={artist}&album={album}&format=json";

            using HttpClient client = new HttpClient();
            string response = await client.GetStringAsync(url);
            using JsonDocument doc = JsonDocument.Parse(response);

            var root = doc.RootElement;
            if (root.TryGetProperty("album", out var albumData))
            {
                if (albumData.TryGetProperty("image", out var imageArray))
                {
                    foreach (var img in imageArray.EnumerateArray())
                    {
                        if (img.TryGetProperty("size", out var size) && size.GetString() == "extralarge")
                        {
                            string coverUrl = img.GetProperty("#text").GetString();
                            if (!string.IsNullOrEmpty(coverUrl))
                            {
                                _apiImageUrl = coverUrl;
                                AlbumCoverImage.Source = ImageSource.FromUri(new Uri(coverUrl));
                                AlbumCoverImage.IsVisible = true;
                                _selectedImage = null;
                            }
                        }
                    }
                }

                // get tracks
                if (albumData.TryGetProperty("tracks", out var tracksProp) && tracksProp.TryGetProperty("track", out var trackData))
                {
                    List<string> trackNames = new List<string>();
                    int totalSeconds = 0;

                    if (trackData.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var track in trackData.EnumerateArray())
                        {
                            if (track.TryGetProperty("name", out var trackName))
                            {
                                trackNames.Add(trackName.GetString());
                            }

                            if (track.TryGetProperty("duration", out var durationProp))
                            {
                                if (durationProp.ValueKind == JsonValueKind.Number)
                                    totalSeconds += durationProp.GetInt32();
                                else if (durationProp.ValueKind == JsonValueKind.String && int.TryParse(durationProp.GetString(), out int d))
                                    totalSeconds += d;
                            }
                        }
                    }
                    else if (trackData.ValueKind == JsonValueKind.Object)
                    {
                        if (trackData.TryGetProperty("name", out var trackName))
                            trackNames.Add(trackName.GetString());

                        if (trackData.TryGetProperty("duration", out var durationProp))
                        {
                            if (durationProp.ValueKind == JsonValueKind.Number)
                                totalSeconds += durationProp.GetInt32();
                            else if (durationProp.ValueKind == JsonValueKind.String && int.TryParse(durationProp.GetString(), out int d))
                                totalSeconds += d;
                        }
                    }

                    TracklistEditor.Text = string.Join("\n", trackNames);

                    if (totalSeconds > 0)
                    {
                        DurationEntry.Text = (totalSeconds / 60).ToString();
                    }
                }

                if (albumData.TryGetProperty("tags", out var tagsProp) && tagsProp.TryGetProperty("tag", out var tagArray))
                {
                    foreach (var tag in tagArray.EnumerateArray())
                    {
                        string tagName = tag.GetProperty("name").GetString()?.ToLower() ?? "";
                        var match = GenrePicker.ItemsSource.Cast<string>().FirstOrDefault(g => g.ToLower() == tagName || tagName.Contains(g.ToLower()));
                        if (match != null)
                        {
                            GenrePicker.SelectedItem = match;
                            break;
                        }
                    }
                }

                // year
                if (albumData.TryGetProperty("wiki", out var wikiProp) && wikiProp.TryGetProperty("summary", out var summaryProp))
                {
                    string summaryText = summaryProp.GetString();
                    var match = Regex.Match(summaryText, @"\b(19\d{2}|20\d{2})\b");
                    if (match.Success)
                    {
                        YearEntry.Text = match.Value;
                    }
                }
            }
            else
            {
                await DisplayAlert("Not Found", "Could not find this album on Last.fm.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("API Error", "Failed to communicate with Last.fm.", "OK");
            Console.WriteLine(ex.Message);
        }
        finally
        {
            btn.Text = "AUTO-FILL FROM LAST.FM";
            btn.IsEnabled = true;
        }
    }

    private void OnShippingClicked(object sender, EventArgs e)
    {
        _selectedFulfillment = "Shipping";
        ShippingBtn.BackgroundColor = Color.FromArgb("#d4af35");
        ShippingBtn.TextColor = Color.FromArgb("#1a1812");
        MeetupBtn.BackgroundColor = Colors.Transparent;
        MeetupBtn.TextColor = Color.FromArgb("#94a3b8");
    }

    private void OnMeetupClicked(object sender, EventArgs e)
    {
        _selectedFulfillment = "Local Meetup";
        MeetupBtn.BackgroundColor = Color.FromArgb("#d4af35");
        MeetupBtn.TextColor = Color.FromArgb("#1a1812");
        ShippingBtn.BackgroundColor = Colors.Transparent;
        ShippingBtn.TextColor = Color.FromArgb("#94a3b8");
    }

    private async void OnSelectImageClicked(object sender, EventArgs e)
    {
        try
        {
            _selectedImage = await MediaPicker.PickPhotoAsync();
            if (_selectedImage != null)
            {
                var stream = await _selectedImage.OpenReadAsync();
                AlbumCoverImage.Source = ImageSource.FromStream(() => stream);
                AlbumCoverImage.IsVisible = true;
                _apiImageUrl = "";
            }
        }
        catch (Exception)
        {
            await DisplayAlert("Error", "Could not pick image", "OK");
        }
    }

    private async void OnPublishClicked(object sender, EventArgs e)
    {
        try
        {
            string uid = Preferences.Get("user_uid", "");
            if (string.IsNullOrEmpty(uid))
            {
                await DisplayAlert("Error", "You must be logged in to post.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(ArtistEntry.Text) ||
                string.IsNullOrWhiteSpace(AlbumEntry.Text) ||
                string.IsNullOrWhiteSpace(YearEntry.Text) ||
                string.IsNullOrWhiteSpace(PriceEntry.Text))
            {
                await DisplayAlert("Missing Info", "Please fill all required fields.", "OK");
                return;
            }

            if (!int.TryParse(YearEntry.Text, out int year) || !double.TryParse(PriceEntry.Text, out double price))
            {
                await DisplayAlert("Format Error", "Year and Price must be numbers.", "OK");
                return;
            }

            Button btn = (Button)sender;
            btn.Text = "UPLOADING...";
            btn.IsEnabled = false;

            string finalImageUrl = "vinyl_placeholder.png";

            if (_selectedImage != null)
            {
                var stream = await _selectedImage.OpenReadAsync();
                finalImageUrl = await new FirebaseStorage("turntable-32899.firebasestorage.app")
                    .Child("Listings")
                    .Child(Guid.NewGuid().ToString() + ".jpg")
                    .PutAsync(stream);
            }
            else if (!string.IsNullOrEmpty(_apiImageUrl))
            {
                finalImageUrl = _apiImageUrl;
            }

            List<string> tracks = new List<string>();
            if (!string.IsNullOrWhiteSpace(TracklistEditor.Text))
            {
                tracks = TracklistEditor.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            ListingModel newListing = new ListingModel
            {
                SellerUid = uid,
                AlbumName = AlbumEntry.Text.Trim(),
                Artist = ArtistEntry.Text.Trim(),
                Genre = GenrePicker.SelectedItem?.ToString() ?? "Not Specified",
                Duration = DurationEntry.Text ?? "00:00",
                ReleaseYear = year,
                Condition = ConditionPicker.SelectedItem?.ToString() ?? "Not Specified",
                Price = price,
                ShippingMethod = _selectedFulfillment,
                ImageUrl = finalImageUrl,
                Tracklist = tracks
            };

            FirebaseService service = new FirebaseService();

            if (_editingListing != null)
            {
                newListing.Id = _editingListing.Id;
                if (_selectedImage == null && string.IsNullOrEmpty(_apiImageUrl))
                {
                    newListing.ImageUrl = _editingListing.ImageUrl;
                }

                await service.UpdateListing(newListing);
                await DisplayAlert("Success", "Vinyl updated!", "OK");
            }
            else
            {
                await service.AddListing(newListing);
                await DisplayAlert("Success", "Your vinyl is now live on Turntable!", "OK");
            }
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "Failed to publish listing.", "OK");
            Console.WriteLine(ex.Message);

            Button btn = (Button)sender;
            btn.Text = "PUBLISH LISTING";
            btn.IsEnabled = true;
        }
    }

    private async void OnCancelTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}