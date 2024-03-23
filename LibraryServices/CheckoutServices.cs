using LibraryData;
using LibraryData.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryServices
{
	public class CheckoutServices : ICheckout
	{
		private LibraryContext _context;

		public CheckoutServices(LibraryContext context)
        {
            _context = context;
        }

        public void Add(Checkout newCheckout)
		{
			_context.Add(newCheckout);
			_context.SaveChanges();
		}

		

		public IEnumerable<Checkout> GetAll()
		{
			return _context.checkouts;
		}

		public Checkout GetById(int checkoutId)
		{
			return GetAll()
				.FirstOrDefault(checkout => checkout.Id == checkoutId);
		}

		public IEnumerable<CheckoutHistory> GetCheckoutHistory(int id)
		{

			return _context.CheckoutHistories
				.Include(h => h.LibraryAsset)
				.Include(h => h.LibraryCard)
				.Where(h => h.LibraryAsset.Id == id);
		}

		

		public IEnumerable<Hold> GetCurrentHolds(int id)
		{
			return _context.Holds
				.Include(h => h.LibraryAsset)
				.Where(h => h.LibraryAsset.Id == id);
		}

		public DateTime GetHoldPlaced(int id)
		{
			throw new NotImplementedException();
		}

		public Checkout GetLatestCheckout(int assetId)
		{
			return _context.checkouts
				.Where(c => c.LibraryAsset.Id == assetId)
				.OrderByDescending(c => c.Since)
				.FirstOrDefault();
		}

		public void MarkFound(int assetId)
		{
			var now = DateTime.Now;
			

			UpdateAssetStatus(assetId, "Available");
			RemoveExistingCheckouts(assetId);
			CloseExistingCheckoutHistory(assetId, now);

		}

		private void UpdateAssetStatus(int assetId, string newStatus)
		{
			var item = _context.LibraryAssets
				.FirstOrDefault(a => a.Id == assetId);

			_context.Update(item);

			item.Status = _context.Statuses
				.FirstOrDefault(status => status.Name == newStatus);
		}

        private void RemoveExistingCheckouts(int assetId)
        {
            //remove any existing checkouts on th item

            var checkout = _context.checkouts
                .FirstOrDefault(co => co.LibraryAsset.Id == assetId);

            if (checkout != null)
            {
                _context.Remove(checkout);
            }
        }

        private void CloseExistingCheckoutHistory(int assetId, DateTime now)
		{
			// Close any existing checkout history
			var history = _context.CheckoutHistories
				.FirstOrDefault(h => h.LibraryAsset.Id == assetId && h.CheckIn == null);

			if (history != null)
			{
				_context.Update(history);
				history.CheckIn = now;
			}

			_context.SaveChanges();
		}

		public void MarkLost(int assetId)
		{
			UpdateAssetStatus(assetId, "Lost");
			_context.SaveChanges();
		}


		public void CheckInItem(int assetId, int libraryCardId)
		{
			var now = DateTime.Now;

			var item = _context.LibraryAssets
				.FirstOrDefault(a => a.Id == assetId);

			//Remove any existing checkouts on the item
			RemoveExistingCheckouts (assetId);

			//Close any existing checkout history
			CloseExistingCheckoutHistory (assetId, now);

			// Look for any existing holds on the item
			 var currentHolds = _context.Holds
				.Include(h => h.LibraryAsset)
				.Include(h => h.LibraryCard)
				.Where(h => h.LibraryAsset.Id == libraryCardId);

			// If there are holds, checkout the item to the libraryCard with the earliest hold.

			if (currentHolds.Any())
			{
				CheckoutToEarliestHold(assetId, currentHolds);
				return;
			}
			
				// Otherwise, update the item status to available
				UpdateAssetStatus(assetId, "Available");

			_context.SaveChanges();
		}


		public void CheckOutItem(int assetId, int libraryCardId)
		{
			var now = DateTime.Now;
			if (IsCheckedOut(assetId))
			{
				return;
				//Add logic here to handle feedback to the user
			}

				var item = _context.LibraryAssets
			   .FirstOrDefault(a => a.Id == assetId);

				UpdateAssetStatus(assetId, "Checked Out");
				var libraryCard = _context.LibraryCards
					.Include(card => card.Checkouts)
					.FirstOrDefault(card => card.Id == libraryCardId);

				var checkout = new Checkout
				{
					LibraryAsset = item,
					LibraryCard = libraryCard,
					Since = now,
					Until = GetDefaultCheckoutTime(now)
				};

				_context.Add(checkout);

				var checkHistory = new CheckoutHistory
				{
					CheckOut = now,
					LibraryAsset = item,
					LibraryCard = libraryCard
				};

				_context.Add(checkHistory);
			_context.SaveChanges();
		}

		public bool IsCheckedOut(int assetId)
		{
			return _context.checkouts
				.Where(co => co.LibraryAsset.Id == assetId)
				.Any();
		}

		private DateTime GetDefaultCheckoutTime(DateTime now)
		{
			return now.AddDays(30);
		}


		private void CheckoutToEarliestHold(int assetId, IQueryable<Hold> currentHolds)
		{
			var earliestHolds = currentHolds
				.OrderBy(holds => holds.HoldPlaced)
				.FirstOrDefault();

			var card = earliestHolds.LibraryCard;

			_context.Remove(earliestHolds);
			_context.SaveChanges();
			CheckOutItem(assetId, card.Id);
		}

		public void PlaceHold(int assetId, int libraryCardId)
		{
			var now = DateTime.Now;

			var asset = _context.LibraryAssets
				.FirstOrDefault(a => a.Id == assetId);

			var card = _context.LibraryCards
				.FirstOrDefault(c => c.Id == libraryCardId);

			if(asset.Status != null && asset.Status.Name == "Available")
			{
				UpdateAssetStatus(assetId, "On Hold");
			}

			var hold = new Hold
			{
				HoldPlaced = now,
				LibraryAsset = asset,
				LibraryCard = card
			};

			_context.Add(hold);
			_context.SaveChanges();
		}


		public string GetCurrentHoldPatronName(int holdId)
		{
			var hold = _context.Holds
				.Include(h => h.LibraryAsset)
				.Include(h => h.LibraryCard)
				.FirstOrDefault(h => h.Id == holdId);

			var cardId = hold?.LibraryCard.Id;

			var patron = _context.Patrons
				.Include(p => p.LibraryCard)
				.FirstOrDefault(p => p.LibraryCard.Id == cardId);

			return patron?.FirstName + " " + patron?.LastName;

		}



		public string GetCurrentCheckoutPatron(int assetId)
		{
			var checkout = GetCheckoutByAssetId(assetId);
			if (checkout == null) {
				return "";
			}

			var cardId = checkout?.LibraryCard.Id;

			var patron  = _context.Patrons
				.Include(p => p.LibraryCard)
				.FirstOrDefault(p => p.LibraryCard.Id == cardId);
			return patron.FirstName + " " + patron.LastName;

		}

		private Checkout GetCheckoutByAssetId(int assetId)
		{
			return _context.checkouts
				.Include(co => co.LibraryAsset)
				.Include(co => co.LibraryCard)
				.FirstOrDefault(co => co.LibraryAsset.Id == assetId);
		}

		public DateTime GetCurrentHoldPlaced(int holdId)
		{
			return
				_context.Holds
				.Include(h => h.LibraryAsset)
				.Include(h => h.LibraryCard)
				.FirstOrDefault(h => h.Id == holdId)
				.HoldPlaced;
		}

	
	}
}
