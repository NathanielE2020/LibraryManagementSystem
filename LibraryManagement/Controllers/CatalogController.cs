using LibraryData;
using LibraryManagement.Models.Catalog;
using LibraryManagement.Models.CheckoutModels;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Controllers
{
    public class CatalogController : Controller
    {
        private ILibraryAsset _assets;
        private ICheckout _checkouts;

        public CatalogController(ILibraryAsset assets, ICheckout checkouts)
        {
            _assets = assets;
            _checkouts = checkouts;
        }

        public IActionResult Index()
        {
            var assetModels = _assets.GetAll();

            var listingResult = assetModels
                .Select(result => new AssetIndexListingModel
                {
                    Id = result.Id,
                    ImageUrl = result.ImageUrl,
                    AuthorORDirector = _assets.GetAuthorOrDirector(result.Id),
                    DeweyCallNumber = _assets.GetDeweyIndex(result.Id),
                    Title = result.Title,
                    Type = _assets.GetType(result.Id),
                });

            var model = new AssetIndexModel()
            {
                Assets = listingResult
            };

            return View(model);
        }

		public IActionResult Detail(int id)
        {
            var asset = _assets.GetById(id);

			var currentHolds = _checkouts.GetCurrentHolds(id)
                .Select(a => new AssetHoldModel
                {
                    HoldPlaced = _checkouts.GetCurrentHoldPlaced(a.Id).ToString("d"),
                    PatronsName = _checkouts.GetCurrentHoldPatronName(a.Id)
                });

            var model = new AssetDetailModel()
            {
                AssetId = id,
                Title = asset.Title,
                Year = asset.Year,
                Cost = asset.Cost,
                Status = asset.Status.Name,
                ImageUrl = asset.ImageUrl,
                AuthorOrDirector = _assets.GetAuthorOrDirector(id),
                CurrentLocation = _assets.GetCurrentLocation(id).Name,
                DeweyCallNumber = _assets.GetDeweyIndex(id),
                ISBN = _assets.GetDeweyIndex(id),
                LatestCheckout = _checkouts.GetLatestCheckout(id),
                PatronsName = _checkouts.GetCurrentHoldPatronName(id),
                CurrentHolds = currentHolds,
                CheckoutHistory = _checkouts.GetCheckoutHistory(id)
            };

            return View(model);
            
        }
    
        public IActionResult CheckOut(int id)
        {
           var asset = _assets.GetById(id);

            var model = new CheckoutModel
            {
                AssetId = id,
                ImageUrl = asset.ImageUrl,
                Title = asset.Title,
                LibraryCardId = "",
                IsCheckedOut = _checkouts.IsCheckedOut(id)
            };

            return View(model);
            
        }

        public IActionResult CheckIn(int assetId, int libraryCardId)
        {
            _checkouts.CheckInItem(assetId, libraryCardId);
            return RedirectToAction("Detail", new { id = assetId });
        }

        public IActionResult Hold(int id)
        {
			var asset = _assets.GetById(id);

            var model = new CheckoutModel
            {
                AssetId = id,
                ImageUrl = asset.ImageUrl,
                Title = asset.Title,
                LibraryCardId = "",
                IsCheckedOut = _checkouts.IsCheckedOut(id),
                HoldCount = _checkouts.GetCurrentHolds(id).Count()
			};

			return View(model);
		}

		public IActionResult MarkLost(int assetId)
		{
			_checkouts.MarkLost(assetId);
			return RedirectToAction("Detail", new { id = assetId });
		}

		public IActionResult MarkFound(int assetId)
		{
			_checkouts.MarkFound(assetId);
			return RedirectToAction("Detail", new { id = assetId });
		}

        [HttpPost]
		public IActionResult PlaceCheckout(int assetId, int libraryCardId)
        {
            _checkouts.CheckOutItem(assetId, libraryCardId);
            return RedirectToAction("Detail", new { id = assetId });
        }


		[HttpPost]
		public IActionResult PlaceHold(int assetId, int libraryCardId)
		{
			_checkouts.PlaceHold(assetId, libraryCardId);
			return RedirectToAction("Detail", new { id = assetId });
		}
	}
}
