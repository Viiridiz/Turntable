using System.Collections.ObjectModel;
using Turntable.Models;
using Turntable.Services;

namespace Turntable.Views.Buyer;

public partial class MyCratePage : ContentPage
{
    public ObservableCollection<ListingModel> CartItems { get; set; } = new ObservableCollection<ListingModel>();
    private bool _isShipping = true;
    private const double TAX_RATE = 0.08;
    private const double FLAT_SHIPPING_RATE = 5.00;

    public MyCratePage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadCart();
    }

    private void LoadCart()
    {
        CartItems.Clear();
        foreach (var item in CartService.CurrentCart)
        {
            CartItems.Add(item);
        }
        CartCollectionView.ItemsSource = CartItems;
        CalculateTotals();
    }

    private void CalculateTotals()
    {
        double subtotal = CartItems.Sum(x => x.Price);
        double tax = subtotal * TAX_RATE;
        double shipping = _isShipping && CartItems.Count > 0 ? FLAT_SHIPPING_RATE : 0.00;
        double total = subtotal + tax + shipping;

        SubtotalLabel.Text = $"${subtotal:F2}";
        TaxLabel.Text = $"${tax:F2}";
        ShippingLabel.Text = $"${shipping:F2}";
        TotalLabel.Text = $"${total:F2}";
    }

    private void OnRemoveTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is ListingModel itemToRemove)
        {
            CartService.RemoveFromCart(itemToRemove);
            CartItems.Remove(itemToRemove);
            CalculateTotals();
        }
    }

    private void OnShippingClicked(object sender, EventArgs e)
    {
        _isShipping = true;
        ShippingBtn.BackgroundColor = Color.FromArgb("#d4af35");
        ShippingBtn.TextColor = Color.FromArgb("#1a1812");
        MeetupBtn.BackgroundColor = Colors.Transparent;
        MeetupBtn.TextColor = Color.FromArgb("#94a3b8");
        AddressEntry.Placeholder = "Delivery Address...";
        CalculateTotals();
    }

    private void OnMeetupClicked(object sender, EventArgs e)
    {
        _isShipping = false;
        MeetupBtn.BackgroundColor = Color.FromArgb("#d4af35");
        MeetupBtn.TextColor = Color.FromArgb("#1a1812");
        ShippingBtn.BackgroundColor = Colors.Transparent;
        ShippingBtn.TextColor = Color.FromArgb("#94a3b8");
        AddressEntry.Placeholder = "Preferred Meetup Location...";
        CalculateTotals();
    }

    private async void OnBackToExploreTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnOrdersTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MyOrdersPage());
    }

    private async void OnProfileTapped(object sender, EventArgs e)
    {
    }

    private async void OnProceedToPaymentClicked(object sender, EventArgs e)
    {
        if (CartItems.Count == 0)
        {
            await DisplayAlert("Empty Crate", "Please add items to your crate before checking out.", "OK");
            return;
        }

        double subtotal = CartItems.Sum(x => x.Price);
        double tax = subtotal * TAX_RATE;
        double total = subtotal + tax;

        await Navigation.PushAsync(new CheckoutPage(CartItems.ToList(), subtotal, tax, total));
    }
}