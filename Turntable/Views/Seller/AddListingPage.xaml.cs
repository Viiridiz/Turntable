using Turntable.Models;
using Turntable.Services;
using Firebase.Storage;

namespace Turntable.Views.Seller;

public partial class AddListingPage : ContentPage
{
    private string _selectedFulfillment = "Shipping";
    private FileResult _selectedImage;
    private ListingModel _editingListing = null;

    public AddListingPage()
    {
        InitializeComponent();
    }

    //edit mode override
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

    // toggles shipping preference
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

    // image upload
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
            }
        }
        catch (Exception)
        {
            await DisplayAlert("Error", "Could not pick image", "OK");
        }
    }

    // input validation
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
                if (_selectedImage == null) newListing.ImageUrl = _editingListing.ImageUrl;

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