using Turntable.Models;
using Turntable.Services;

namespace Turntable.Views.Seller;

public partial class MySalesPage : ContentPage
{
    private FirebaseService _firebaseService = new FirebaseService();
    private List<OrderModel> _allSales = new List<OrderModel>();

    public MySalesPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadSales();
    }

    private async Task LoadSales()
    {
        string uid = Preferences.Get("user_uid", "");
        if (!string.IsNullOrEmpty(uid))
        {
            _allSales = await _firebaseService.GetSellerOrders(uid);
            ApplyFilter("ALL SALES");
        }
    }

    private void OnFilterClicked(object sender, EventArgs e)
    {
        var btn = sender as Button;
        ApplyFilter(btn.Text);

        // UI Feedback: Highlight selected button
        AllBtn.BackgroundColor = ProcessingBtn.BackgroundColor = ShippedBtn.BackgroundColor = DeliveredBtn.BackgroundColor = Colors.Transparent;
        AllBtn.TextColor = ProcessingBtn.TextColor = ShippedBtn.TextColor = DeliveredBtn.TextColor = Color.FromArgb("#94a3b8");

        btn.BackgroundColor = Color.FromArgb("#d4af35");
        btn.TextColor = Color.FromArgb("#1a1812");
    }

    private void ApplyFilter(string status)
    {
        if (status == "ALL SALES")
            SalesList.ItemsSource = _allSales.OrderByDescending(o => o.OrderDate).ToList();
        else
            SalesList.ItemsSource = _allSales.Where(o => o.Status == status).OrderByDescending(o => o.OrderDate).ToList();
    }

    private async void OnUpdateStatusClicked(object sender, EventArgs e)
    {
        if ((sender as Button)?.CommandParameter is OrderModel order)
        {
            string action = await DisplayActionSheet("Update Order Status", "Cancel", null, "PROCESSING", "SHIPPED", "DELIVERED");
            if (action == "Cancel" || action == null) return;

            order.Status = action;
            await _firebaseService.UpdateOrder(order);
            await LoadSales();
        }
    }

    private async void OnMyInventoryTapped(object sender, EventArgs e) => await Navigation.PushAsync(new MyInventoryPage());
    private async void OnDashboardTapped(object sender, EventArgs e) { /* dashboard logic */ }
    private async void OnProfileTapped(object sender, EventArgs e) { /* profile logic */ }
}