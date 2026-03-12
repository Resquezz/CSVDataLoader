using System.Globalization;
namespace CrewRedTestTask.Domain.Entities;

public readonly record struct TaxiTripRecord(
	DateTime PickupDateUtc,
	DateTime DropoffDateUtc,
	byte PassengerCount,
	decimal TripDistance,
	string StoreAndFwdFlag,
	short PickupLocationId,
	short DropoffLocationId,
	decimal FareAmount,
	decimal TipAmount)
{
	public string[] ToDuplicateRow() =>
	[
		PickupDateUtc.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
		DropoffDateUtc.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
		PassengerCount.ToString(CultureInfo.InvariantCulture),
		TripDistance.ToString("0.##", CultureInfo.InvariantCulture),
		StoreAndFwdFlag,
		PickupLocationId.ToString(CultureInfo.InvariantCulture),
		DropoffLocationId.ToString(CultureInfo.InvariantCulture),
		FareAmount.ToString("0.##", CultureInfo.InvariantCulture),
		TipAmount.ToString("0.##", CultureInfo.InvariantCulture)
	];
}
