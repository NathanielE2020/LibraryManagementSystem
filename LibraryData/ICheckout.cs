using LibraryData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryData
{
	public interface ICheckout
	{
		void Add(Checkout Checkout);
		IEnumerable<Checkout> GetAll();
		IEnumerable<CheckoutHistory> GetCheckoutHistory(int id);
		IEnumerable<Hold> GetCurrentHolds(int id);

		Checkout GetById(int checkedId);
		Checkout GetLatestCheckout(int assetId);
		string GetCurrentCheckoutPatron(int assetId);
		string GetCurrentHoldPatronName(int id);
		DateTime GetCurrentHoldPlaced(int id);

		void CheckOutItem(int assetId, int libraryCardId);
		void CheckInItem(int assetId, int libraryCardId);
		void PlaceHold(int assetId, int libraryCardId);
		void MarkLost(int assetId);
		void MarkFound(int assetId);
		bool IsCheckedOut(int id);
	}
}
