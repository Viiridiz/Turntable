using Turntable.Models;
using Turntable.Services;

namespace Turntable.Views.Buyer;

public partial class CheckoutPage : ContentPage
{
    private List<ListingModel> _checkoutItems;
    private double _totalPrice;
    private FirebaseService _firebaseService = new FirebaseService();

    public CheckoutPage(List<ListingModel> itemsToPurchase, double subtotal, double tax, double total)
    {
        InitializeComponent();
        _checkoutItems = itemsToPurchase;
        _totalPrice = total;

        ItemsTotalLabel.Text = $"${subtotal:F2}";
        TaxLabel.Text = $"${tax:F2}";
        GrandTotalLabel.Text = $"${_totalPrice:F2}";
    }

    private async void OnConfirmPurchaseClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CardNumberEntry.Text) ||
            string.IsNullOrWhiteSpace(ExpiryEntry.Text) ||
            string.IsNullOrWhiteSpace(CvvEntry.Text))
        {
            await DisplayAlert("Required", "Please fill in all payment details to proceed.", "OK");
            return;
        }

        string buyerUid = Preferences.Get("user_uid", "");
        if (string.IsNullOrEmpty(buyerUid))
        {
            await DisplayAlert("Error", "You must be logged in to checkout.", "OK");
            return;
        }

        ConfirmBtn.Text = "VALIDATING...";
        ConfirmBtn.IsEnabled = false;

        try
        {
            // edge case to handle new updates
            var freshListings = await _firebaseService.GetListings();

            foreach (var item in _checkoutItems)
            {
                var realTimeItem = freshListings.FirstOrDefault(x => x.Id == item.Id);

                if (realTimeItem == null || realTimeItem.IsSold)
                {
                    await DisplayAlert("Item Unavailable", $"Oops! '{item.AlbumName}' was just sold to someone else or removed by the seller.", "OK");
                    CartService.RemoveFromCart(item);
                    await Navigation.PopAsync();
                    return;
                }

                if (realTimeItem.Price != item.Price)
                {
                    await DisplayAlert("Price Changed", $"The seller recently updated the price for '{item.AlbumName}'. Please review your cart.", "OK");
                    CartService.RemoveFromCart(item);
                    CartService.AddToCart(realTimeItem);
                    await Navigation.PopAsync();
                    return;
                }
            }

            ConfirmBtn.Text = "PROCESSING...";
            Random rnd = new Random();

            foreach (var item in _checkoutItems)
            {
                OrderModel newOrder = new OrderModel
                {
                    OrderId = $"TRN-{rnd.Next(1000, 9999)}",
                    BuyerUid = buyerUid,
                    SellerUid = item.SellerUid,
                    ListingId = item.Id,
                    AlbumName = item.AlbumName,
                    Artist = item.Artist,
                    ImageUrl = item.ImageUrl,
                    PurchasePrice = item.Price,
                    FulfillmentMethod = item.ShippingMethod,
                    DeliveryDetails = "Standard Checkout",
                    OrderDate = DateTime.Now,
                    Status = "PROCESSING"
                };

                await _firebaseService.CreateOrder(newOrder);

                item.IsSold = true;
                await _firebaseService.UpdateListing(item);
            }

            CartService.ClearCart();

            await DisplayAlert("Success!", "Your payment has been processed. You can track your items in My Orders.", "OK");

            await Navigation.PushAsync(new MyOrdersPage());
        }
        catch (Exception ex)
        {
            await DisplayAlert("Transaction Failed", $"There was an issue processing your order: {ex.Message}", "OK");
            ConfirmBtn.Text = "CONFIRM PURCHASE";
            ConfirmBtn.IsEnabled = true;
        }
    }

    private async void OnCancelTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnMarketplaceTapped(object sender, EventArgs e)
    {
        await Navigation.PopToRootAsync();
    }

    private async void OnOrdersTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MyOrdersPage());
    }

    private async void OnCartTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnProfileTapped(object sender, EventArgs e)
    {
    }
}