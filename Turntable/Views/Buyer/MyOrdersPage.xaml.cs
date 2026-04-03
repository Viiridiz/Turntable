using Turntable.Models;
using Turntable.Services;

namespace Turntable.Views.Buyer;

public partial class MyOrdersPage : ContentPage
{
    private FirebaseService _firebaseService = new FirebaseService();
    private List<OrderModel> _allOrders = new List<OrderModel>();

    public MyOrdersPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadOrders();
    }

    private async Task LoadOrders()
    {
        string uid = Preferences.Get("user_uid", "");
        if (!string.IsNullOrEmpty(uid))
        {
            try
            {
                // Fetch and cache all orders for filtering
                _allOrders = await _firebaseService.GetBuyerOrders(uid);
                ApplyFilter("ALL ORDERS");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Could not load orders.", "OK");
                Console.WriteLine(ex.Message);
            }
        }
    }

    private void OnFilterClicked(object sender, EventArgs e)
    {
        var btn = sender as Button;
        ApplyFilter(btn.Text);

        // UI Reset: Set all buttons back to transparent background / grey text
        AllBtn.BackgroundColor = ProcessingBtn.BackgroundColor = ShippedBtn.BackgroundColor = DeliveredBtn.BackgroundColor = Colors.Transparent;
        AllBtn.TextColor = ProcessingBtn.TextColor = ShippedBtn.TextColor = DeliveredBtn.TextColor = Color.FromArgb("#94a3b8");

        // UI Active: Highlight the specific button clicked
        btn.BackgroundColor = Color.FromArgb("#d4af35");
        btn.TextColor = Color.FromArgb("#1a1812");
    }

    private void ApplyFilter(string status)
    {
        if (status == "ALL ORDERS")
        {
            OrdersList.ItemsSource = _allOrders.OrderByDescending(o => o.OrderDate).ToList();
        }
        else
        {
            OrdersList.ItemsSource = _allOrders.Where(o => o.Status == status).OrderByDescending(o => o.OrderDate).ToList();
        }
    }


    private async void OnMarketplaceTapped(object sender, EventArgs e)
    {
        await Navigation.PopToRootAsync();
    }

    private async void OnCartTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MyCratePage());
    }

    private async void OnProfileTapped(object sender, EventArgs e)
    {
        // await Navigation.PushAsync(new ProfilePage());
    }
}