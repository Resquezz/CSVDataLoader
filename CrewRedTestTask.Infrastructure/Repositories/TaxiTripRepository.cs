using System.Data;
using CrewRedTestTask.Application.Configuration;
using CrewRedTestTask.Infrastructure.Data;
using CrewRedTestTask.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using CrewRedTestTask.Domain.Interfaces;

namespace CrewRedTestTask.Infrastructure.Repositories;

public sealed class TaxiTripRepository : ITaxiTripRepository
{
	private const int BatchSize = 10_000;
	private readonly IDbContextFactory<TaxiTripsDbContext> _dbContextFactory;
	private readonly CliOptions _options;

	public TaxiTripRepository(IDbContextFactory<TaxiTripsDbContext> dbContextFactory, CliOptions options)
	{
		_dbContextFactory = dbContextFactory;
		_options = options;
	}

	public async Task InitializeAsync(CancellationToken cancellationToken = default)
	{
		await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
		await dbContext.Database.EnsureCreatedAsync(cancellationToken);
	}

	public async Task<int> BulkInsertAsync(IReadOnlyCollection<TaxiTripRecord> records, CancellationToken cancellationToken = default)
	{
		if (records.Count == 0)
		{
			return 0;
		}

		await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
		var sqlConnection = (SqlConnection)dbContext.Database.GetDbConnection();

		if (sqlConnection.State != ConnectionState.Open)
		{
			await sqlConnection.OpenAsync(cancellationToken);
		}

		using var bulkCopy = CreateBulkCopy(sqlConnection);
		using var dataTable = CreateBatchTable(records);
		await bulkCopy.WriteToServerAsync(dataTable, cancellationToken);
		return records.Count;
	}

	private SqlBulkCopy CreateBulkCopy(SqlConnection connection)
	{
		var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.TableLock | SqlBulkCopyOptions.CheckConstraints, null)
		{
			DestinationTableName = _options.DestinationTable,
			BulkCopyTimeout = 0,
			BatchSize = BatchSize,
			EnableStreaming = true
		};

		bulkCopy.ColumnMappings.Add("tpep_pickup_datetime", "tpep_pickup_datetime");
		bulkCopy.ColumnMappings.Add("tpep_dropoff_datetime", "tpep_dropoff_datetime");
		bulkCopy.ColumnMappings.Add("passenger_count", "passenger_count");
		bulkCopy.ColumnMappings.Add("trip_distance", "trip_distance");
		bulkCopy.ColumnMappings.Add("store_and_fwd_flag", "store_and_fwd_flag");
		bulkCopy.ColumnMappings.Add("PULocationID", "PULocationID");
		bulkCopy.ColumnMappings.Add("DOLocationID", "DOLocationID");
		bulkCopy.ColumnMappings.Add("fare_amount", "fare_amount");
		bulkCopy.ColumnMappings.Add("tip_amount", "tip_amount");

		return bulkCopy;
	}

	private static DataTable CreateBatchTable(IEnumerable<TaxiTripRecord> records)
	{
		var table = new DataTable();
		table.Columns.Add("tpep_pickup_datetime", typeof(DateTime));
		table.Columns.Add("tpep_dropoff_datetime", typeof(DateTime));
		table.Columns.Add("passenger_count", typeof(byte));
		table.Columns.Add("trip_distance", typeof(decimal));
		table.Columns.Add("store_and_fwd_flag", typeof(string));
		table.Columns.Add("PULocationID", typeof(short));
		table.Columns.Add("DOLocationID", typeof(short));
		table.Columns.Add("fare_amount", typeof(decimal));
		table.Columns.Add("tip_amount", typeof(decimal));

		foreach (var record in records)
		{
			table.Rows.Add(
				record.PickupDateUtc,
				record.DropoffDateUtc,
				record.PassengerCount,
				record.TripDistance,
				record.StoreAndFwdFlag,
				record.PickupLocationId,
				record.DropoffLocationId,
				record.FareAmount,
				record.TipAmount);
		}

		return table;
	}
}