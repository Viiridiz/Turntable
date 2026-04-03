using Firebase.Database;
using Firebase.Database.Query;
using Turntable.Models;

namespace Turntable.Services;

public class FirebaseService
{
    private readonly FirebaseClient firebase = new FirebaseClient("https://turntable-32899-default-rtdb.firebaseio.com/");

    // adds a new listing
    public async Task AddListing(ListingModel listing)
    {
        await firebase
            .Child("Listings")
            .PostAsync(listing);
    }

    public async Task<List<ListingModel>> GetListings()
    {
        var items = await firebase
            .Child("Listings")
            .OnceAsync<ListingModel>();

        return items.Select(item => new ListingModel
        {
            Id = item.Key,
            IsSold = item.Object.IsSold,
            SellerUid = item.Object.SellerUid,
            AlbumName = item.Object.AlbumName,
            Artist = item.Object.Artist,
            Genre = item.Object.Genre,
            Duration = item.Object.Duration,
            ReleaseYear = item.Object.ReleaseYear,
            Condition = item.Object.Condition,
            Price = item.Object.Price,
            ShippingMethod = item.Object.ShippingMethod,
            ImageUrl = item.Object.ImageUrl,
            Tracklist = item.Object.Tracklist ?? new List<string>()
        }).ToList();
    }

    // update
    public async Task UpdateListing(ListingModel listing)
    {
        await firebase
            .Child("Listings")
            .Child(listing.Id)
            .PutAsync(listing);
    }

    // delete
    public async Task DeleteListing(string id)
    {
        await firebase
            .Child("Listings")
            .Child(id)
            .DeleteAsync();
    }

    // orders
    public async Task CreateOrder(OrderModel order)
    {
        await firebase
            .Child("Orders")
            .PostAsync(order);
    }

    public async Task<List<OrderModel>> GetBuyerOrders(string buyerUid)
    {
        var items = await firebase
            .Child("Orders")
            .OnceAsync<OrderModel>();

        return items.Select(item => new OrderModel
        {
            OrderId = item.Object.OrderId,
            BuyerUid = item.Object.BuyerUid,
            SellerUid = item.Object.SellerUid,
            ListingId = item.Object.ListingId,
            AlbumName = item.Object.AlbumName,
            Artist = item.Object.Artist,
            ImageUrl = item.Object.ImageUrl,
            PurchasePrice = item.Object.PurchasePrice,
            FulfillmentMethod = item.Object.FulfillmentMethod,
            DeliveryDetails = item.Object.DeliveryDetails,
            OrderDate = item.Object.OrderDate,
            Status = item.Object.Status
        }).Where(o => o.BuyerUid == buyerUid).ToList();
    }

    public async Task<List<OrderModel>> GetSellerOrders(string sellerUid)
    {
        var items = await firebase
            .Child("Orders")
            .OnceAsync<OrderModel>();

        return items.Select(item => new OrderModel
        {
            OrderId = item.Object.OrderId,
            BuyerUid = item.Object.BuyerUid,
            SellerUid = item.Object.SellerUid,
            ListingId = item.Object.ListingId,
            AlbumName = item.Object.AlbumName,
            Artist = item.Object.Artist,
            ImageUrl = item.Object.ImageUrl,
            PurchasePrice = item.Object.PurchasePrice,
            FulfillmentMethod = item.Object.FulfillmentMethod,
            DeliveryDetails = item.Object.DeliveryDetails,
            OrderDate = item.Object.OrderDate,
            Status = item.Object.Status
        }).Where(o => o.SellerUid == sellerUid).ToList();
    }

    public async Task UpdateOrder(OrderModel order)
    {
        var toUpdate = (await firebase.Child("Orders").OnceAsync<OrderModel>())
            .FirstOrDefault(a => a.Object.OrderId == order.OrderId);

        if (toUpdate != null)
        {
            await firebase.Child("Orders").Child(toUpdate.Key).PutAsync(order);
        }
    }
}