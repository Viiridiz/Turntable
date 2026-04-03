using System;

namespace Turntable.Models
{
    public class OrderModel
    {
        public string OrderId { get; set; }

        public string BuyerUid { get; set; }
        public string SellerUid { get; set; }

        public string ListingId { get; set; }
        public string AlbumName { get; set; }
        public string Artist { get; set; }
        public string ImageUrl { get; set; }
        public double PurchasePrice { get; set; }
        public string FulfillmentMethod { get; set; } // shipping or meetup
        public string DeliveryDetails { get; set; } 

        // State
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } 
    }
}