using Turntable.Models;
using Turntable.Services;

namespace Turntable.Views.Buyer;

public partial class MarketplacePage : ContentPage
{
    FirebaseService service = new FirebaseService();
    private List<ListingModel> _allActiveListings = new List<ListingModel>();

    public MarketplacePage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadMarketplace();
    }

    private async Task LoadMarketplace()
    {
        try
        {
            var allListings = await service.GetListings();
            _allActiveListings = allListings.Where(album => !album.IsSold).ToList();
            ApplyFilters();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not load marketplace: {ex.Message}", "OK");
        }
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e) => ApplyFilters();
    private void OnFilterChanged(object sender, EventArgs e) => ApplyFilters();

    private void ApplyFilters()
    {
        string searchText = SearchEntry.Text?.ToLower() ?? "";
        string conditionFilter = ConditionPicker.SelectedItem?.ToString() ?? "All Conditions";
        string genreFilter = GenrePicker.SelectedItem?.ToString() ?? "All Genres";
        string sortType = SortPicker.SelectedItem?.ToString() ?? "";

        var filtered = _allActiveListings.Where(x =>
            !x.IsSold && 
            ((x.AlbumName?.ToLower().Contains(searchText) == true) ||
             (x.Artist?.ToLower().Contains(searchText) == true))).ToList();

        if (conditionFilter != "All Conditions")
            filtered = filtered.Where(x => x.Condition == conditionFilter).ToList();

        if (genreFilter != "All Genres")
            filtered = filtered.Where(x => x.Genre == genreFilter).ToList();

        switch (sortType)
        {
            case "Price: Low to High": filtered = filtered.OrderBy(x => x.Price).ToList(); break;
            case "Price: High to Low": filtered = filtered.OrderByDescending(x => x.Price).ToList(); break;
            case "Year: Newest": filtered = filtered.OrderByDescending(x => x.ReleaseYear).ToList(); break;
            case "Year: Oldest": filtered = filtered.OrderBy(x => x.ReleaseYear).ToList(); break;
        }

        MarketplaceList.ItemsSource = filtered;
    }

    private async void OnListingSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is ListingModel selectedListing)
        {
            MarketplaceList.SelectedItem = null;
            await Navigation.PushAsync(new ListingDetailPage(selectedListing));
        }
    }

    private async void OnCartTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MyCratePage());
    }

    private async void OnMyOrdersTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MyOrdersPage());
    }
}