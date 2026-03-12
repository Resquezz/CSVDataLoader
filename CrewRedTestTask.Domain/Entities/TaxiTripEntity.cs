namespace CrewRedTestTask.Domain.Entities;

public sealed class TaxiTripEntity
{
	public long Id { get; init; }
	public DateTime PickupDateUtc { get; init; }
	public DateTime DropoffDateUtc { get; init; }
	public byte PassengerCount { get; init; }
	public decimal TripDistance { get; init; }
	public string StoreAndFwdFlag { get; init; } = string.Empty;
	public short PickupLocationId { get; init; }
	public short DropoffLocationId { get; init; }
	public decimal FareAmount { get; init; }
	public decimal TipAmount { get; init; }
	public long TripDurationSeconds { get; init; }
}
