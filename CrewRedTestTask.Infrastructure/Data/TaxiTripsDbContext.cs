using Microsoft.EntityFrameworkCore;
using CrewRedTestTask.Domain.Entities;

namespace CrewRedTestTask.Infrastructure.Data;
public sealed class TaxiTripsDbContext : DbContext
{
	public TaxiTripsDbContext(DbContextOptions<TaxiTripsDbContext> options)
		: base(options)
	{
	}

	public DbSet<TaxiTripEntity> TaxiTrips => Set<TaxiTripEntity>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		var entity = modelBuilder.Entity<TaxiTripEntity>();
		entity.ToTable("TaxiTrips", "dbo");
		entity.HasKey(record => record.Id);
		entity.Property(record => record.Id).ValueGeneratedOnAdd();
		entity.Property(record => record.PickupDateUtc).HasColumnName("tpep_pickup_datetime").HasColumnType("datetime2(0)");
		entity.Property(record => record.DropoffDateUtc).HasColumnName("tpep_dropoff_datetime").HasColumnType("datetime2(0)");
		entity.Property(record => record.PassengerCount).HasColumnName("passenger_count");
		entity.Property(record => record.TripDistance).HasColumnName("trip_distance").HasPrecision(9, 2);
		entity.Property(record => record.StoreAndFwdFlag).HasColumnName("store_and_fwd_flag").HasMaxLength(3).IsUnicode(false);
		entity.Property(record => record.PickupLocationId).HasColumnName("PULocationID");
		entity.Property(record => record.DropoffLocationId).HasColumnName("DOLocationID");
		entity.Property(record => record.FareAmount).HasColumnName("fare_amount").HasPrecision(10, 2);
		entity.Property(record => record.TipAmount).HasColumnName("tip_amount").HasPrecision(10, 2);
		entity.Property(record => record.TripDurationSeconds)
			.HasColumnName("trip_duration_seconds")
			.HasComputedColumnSql("DATEDIFF_BIG(SECOND, [tpep_pickup_datetime], [tpep_dropoff_datetime])", stored: true);

		entity.HasIndex(record => record.PickupLocationId)
			.HasDatabaseName("IX_TaxiTrips_PULocationID_TipAmount")
			.IncludeProperties(record => new { record.TipAmount });

		entity.HasIndex(record => record.TripDistance)
			.IsDescending()
			.HasDatabaseName("IX_TaxiTrips_TripDistance")
			.IncludeProperties(record => new
			{
				record.FareAmount,
				record.PickupLocationId,
				record.DropoffLocationId,
				record.PickupDateUtc,
				record.DropoffDateUtc,
				record.PassengerCount,
				record.StoreAndFwdFlag,
				record.TipAmount
			});

		entity.HasIndex(record => record.TripDurationSeconds)
			.IsDescending()
			.HasDatabaseName("IX_TaxiTrips_TripDurationSeconds")
			.IncludeProperties(record => new
			{
				record.TripDistance,
				record.FareAmount,
				record.PickupLocationId,
				record.DropoffLocationId,
				record.PickupDateUtc,
				record.DropoffDateUtc,
				record.PassengerCount,
				record.StoreAndFwdFlag,
				record.TipAmount
			});

		entity.HasIndex(record => new { record.PickupDateUtc, record.DropoffDateUtc, record.PassengerCount })
			.HasDatabaseName("IX_TaxiTrips_PickupDropoffPassenger");
	}
}
