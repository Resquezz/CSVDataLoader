namespace CrewRedTestTask.Domain.Entities;

public readonly record struct TripDeduplicationKey(DateTime PickupDateUtc, DateTime DropoffDateUtc, byte PassengerCount);
