using Turntable.Models;

namespace Turntable.Views.Buyer;

public partial class ListingDetailPage : ContentPage
{
    private ListingModel _listing;

    public ListingDetailPage(ListingModel listing)
    {
        InitializeComponent();
        _listing = listing;
        PopulateUI();
    }

    private void PopulateUI()
    {
        BreadcrumbGenre.Text = _listing.Genre;
        BreadcrumbArtist.Text = _listing.Artist;

        CoverImage.Source = _listing.ImageUrl;
        TitleLabel.Text = _listing.AlbumName.ToUpper();
        ArtistLabel.Text = _listing.Artist;
        PriceLabel.Text = $"${_listing.Price:F2}";
        ConditionLabel.Text = _listing.Condition;
        YearLabel.Text = _listing.ReleaseYear.ToString();
        DurationLabel.Text = $"{_listing.Duration} mins";
        FulfillmentLabel.Text = _listing.ShippingMethod;

        PopulateTracklist();
    }

    private void PopulateTracklist()
    {
        if (_listing.Tracklist == null || _listing.Tracklist.Count == 0) return;

        //sideab split
        int halfPoint = (int)Math.Ceiling(_listing.Tracklist.Count / 2.0);
        var sideA = _listing.Tracklist.Take(halfPoint).ToList();
        var sideB = _listing.Tracklist.Skip(halfPoint).ToList();

        int index = 1;
        foreach (var track in sideA)
        {
            SideALayout.Children.Add(CreateTrackRow(index.ToString("D2"), track));
            index++;
        }

        foreach (var track in sideB)
        {
            SideBLayout.Children.Add(CreateTrackRow(index.ToString("D2"), track));
            index++;
        }
    }

    private HorizontalStackLayout CreateTrackRow(string number, string name)
    {
        return new HorizontalStackLayout
        {
            Spacing = 15,
            Children =
            {
                new Label { Text = number, TextColor = Color.FromArgb("#64748b"), FontSize = 14 },
                new Label { Text = name, TextColor = Colors.White, FontSize = 14 }
            }
        };
    }

    private async void OnAddToCrateClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Added to Crate", $"{_listing.AlbumName} has been added to your crate.", "OK");
    }

    private async void OnBackTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}