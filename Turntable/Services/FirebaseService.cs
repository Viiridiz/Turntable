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
}