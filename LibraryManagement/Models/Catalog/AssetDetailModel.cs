﻿using LibraryData.Models;
namespace LibraryManagement.Models.Catalog
{
	public class AssetDetailModel
	{
		public int AssetId { get; set; }
		public string Title { get; set; }
        public string AuthorOrDirector { get; set; }
        public string Type { get; set; }
        public int Year { get; set; }
        public string ISBN { get; set; }
        public string DeweyCallNumber { get; set; }
        public string Status { get; set; }
        public decimal Cost { get; set; }
        public string CurrentLocation { get; set; }
        public string ImageUrl { get; set; }
        public string PatronsName { get; set; }
		public LibraryData.Models.Checkout LatestCheckout { get; set; }
		public IEnumerable<CheckoutHistory> CheckoutHistory { get; set; }
        public IEnumerable<AssetHoldModel> CurrentHolds { get; set; }
    }

    public class AssetHoldModel
    {
        public string PatronsName { get; set; }
        public string HoldPlaced { get; set; }
    }
}
