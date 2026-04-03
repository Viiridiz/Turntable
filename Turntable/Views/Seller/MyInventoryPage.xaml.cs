using Turntable.Models;
using Turntable.Services;

namespace Turntable.Views.Seller;

public partial class MyInventoryPage : ContentPage
{
    FirebaseService service = new FirebaseService();
    private List<ListingModel> _allMyListings = new List<ListingModel>();
    private bool _showActive = true;

    public MyInventoryPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadInventory();
    }

    private async Task LoadInventory()
    {
        try
        {
            string currentUid = Preferences.Get("user_uid", "");
            var allListings = await service.GetListings();
            _allMyListings = allListings.Where(album => album.SellerUid == currentUid).ToList();

            ApplyFilters();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not load inventory: {ex.Message}", "OK");
        }
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        ApplyFilters();
    }

    private void OnFilterChanged(object sender, EventArgs e)
    {
        ApplyFilters();
    }

    private void OnActiveTabTapped(object sender, EventArgs e)
    {
        _showActive = true;
        ActiveTab.BackgroundColor = Color.FromArgb("#433d28");
        ActiveLabel.TextColor = Colors.White;

        SoldTab.BackgroundColor = Colors.Transparent;
        SoldIndicator.BackgroundColor = Colors.Transparent;
        SoldLabel.TextColor = Color.FromArgb("#cbd5e1");

        ApplyFilters();
    }

    private void OnSoldTabTapped(object sender, EventArgs e)
    {
        _showActive = false;
        SoldTab.BackgroundColor = Color.FromArgb("#433d28");
        SoldLabel.TextColor = Colors.White;

        ActiveTab.BackgroundColor = Colors.Transparent;
        ActiveIndicator.BackgroundColor = Colors.Transparent;
        ActiveLabel.TextColor = Color.FromArgb("#cbd5e1");

        ApplyFilters();
    }

    // seearch filters
    private void ApplyFilters()
    {
        string searchText = SearchEntry.Text?.ToLower() ?? "";
        string conditionFilter = ConditionFilterPicker.SelectedItem?.ToString() ?? "All Conditions";
        string genreFilter = GenreFilterPicker.SelectedItem?.ToString() ?? "All Genres";
        string sortType = SortPicker.SelectedItem?.ToString() ?? "";

        var filtered = _allMyListings.Where(x =>
            x.IsSold == !_showActive &&
            ((x.AlbumName?.ToLower().Contains(searchText) == true) ||
             (x.Artist?.ToLower().Contains(searchText) == true))).ToList();

        if (conditionFilter != "All Conditions")
        {
            filtered = filtered.Where(x => x.Condition == conditionFilter).ToList();
        }

        if (genreFilter != "All Genres")
        {
            filtered = filtered.Where(x => x.Genre == genreFilter).ToList();
        }

        switch (sortType)
        {
            case "Price: Low to High":
                filtered = filtered.OrderBy(x => x.Price).ToList();
                break;
            case "Price: High to Low":
                filtered = filtered.OrderByDescending(x => x.Price).ToList();
                break;
            case "Year: Newest":
                filtered = filtered.OrderByDescending(x => x.ReleaseYear).ToList();
                break;
            case "Year: Oldest":
                filtered = filtered.OrderBy(x => x.ReleaseYear).ToList();
                break;
        }

        ListingList.ItemsSource = filtered;
    }

    private async void OnPostListingClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AddListingPage());
    }

    private async void OnEditClicked(object sender, EventArgs e)
    {
        Button btn = (Button)sender;
        ListingModel selectedListing = (ListingModel)btn.BindingContext;

        await Navigation.PushAsync(new AddListingPage(selectedListing));
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        Button btn = (Button)sender;
        ListingModel selectedListing = (ListingModel)btn.BindingContext;

        bool confirm = await DisplayAlert("Delete?", $"Are you sure you want to delete {selectedListing.AlbumName}?", "Yes", "No");

        if (confirm)
        {
            await service.DeleteListing(selectedListing.Id);
            await LoadInventory();
        }
    }

    private async void OnDashboardTapped(object sender, EventArgs e)
    {

    }

    private async void OnOrdersTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MySalesPage());
    }

    private async void OnProfileTapped(object sender, EventArgs e)
    {
    }
}