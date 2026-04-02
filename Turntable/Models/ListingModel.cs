using System;
using System.Collections.Generic;

namespace Turntable.Models
{
    public class ListingModel
    {
        public string Id { get; set; }
        public string SellerUid { get; set; }
        public string AlbumName { get; set; }
        public string Artist { get; set; }
        public string Genre { get; set; }
        public string Duration { get; set; }
        public int ReleaseYear { get; set; }
        public string Condition { get; set; }
        public double Price { get; set; }
        public string ShippingMethod { get; set; }
        public string ImageUrl { get; set; }
        public List<string> Tracklist { get; set; } = new List<string>();
        public bool IsSold { get; set; } = false;
    }
}