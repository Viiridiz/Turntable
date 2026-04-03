using System.Collections.Generic;
using Turntable.Models;

namespace Turntable.Services
{
    public static class CartService
    {
        public static List<ListingModel> CurrentCart { get; set; } = new List<ListingModel>();

        public static void AddToCart(ListingModel item)
        {
            CurrentCart.Add(item);
        }

        public static void RemoveFromCart(ListingModel item)
        {
            CurrentCart.Remove(item);
        }

        public static void ClearCart()
        {
            CurrentCart.Clear();
        }
    }
}