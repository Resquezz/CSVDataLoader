USE [CrewRedTestTaskDb];
GO

IF OBJECT_ID(N'dbo.TaxiTrips', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TaxiTrips
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TaxiTrips PRIMARY KEY CLUSTERED,
        tpep_pickup_datetime DATETIME2(0) NOT NULL,
        tpep_dropoff_datetime DATETIME2(0) NOT NULL,
        passenger_count TINYINT NOT NULL,
        trip_distance DECIMAL(9,2) NOT NULL,
        store_and_fwd_flag VARCHAR(3) NOT NULL,
        PULocationID SMALLINT NOT NULL,
        DOLocationID SMALLINT NOT NULL,
        fare_amount DECIMAL(10,2) NOT NULL,
        tip_amount DECIMAL(10,2) NOT NULL,
        trip_duration_seconds AS DATEDIFF_BIG(SECOND, tpep_pickup_datetime, tpep_dropoff_datetime) PERSISTED
    );
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_TaxiTrips_PULocationID_TipAmount'
      AND object_id = OBJECT_ID(N'dbo.TaxiTrips')
)
BEGIN
    CREATE INDEX IX_TaxiTrips_PULocationID_TipAmount
        ON dbo.TaxiTrips (PULocationID)
        INCLUDE (tip_amount);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_TaxiTrips_TripDistance'
      AND object_id = OBJECT_ID(N'dbo.TaxiTrips')
)
BEGIN
    CREATE INDEX IX_TaxiTrips_TripDistance
        ON dbo.TaxiTrips (trip_distance DESC)
        INCLUDE (fare_amount, PULocationID, DOLocationID, tpep_pickup_datetime, tpep_dropoff_datetime, passenger_count, store_and_fwd_flag, tip_amount);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_TaxiTrips_TripDurationSeconds'
      AND object_id = OBJECT_ID(N'dbo.TaxiTrips')
)
BEGIN
    CREATE INDEX IX_TaxiTrips_TripDurationSeconds
        ON dbo.TaxiTrips (trip_duration_seconds DESC)
        INCLUDE (trip_distance, fare_amount, PULocationID, DOLocationID, tpep_pickup_datetime, tpep_dropoff_datetime, passenger_count, store_and_fwd_flag, tip_amount);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_TaxiTrips_PickupDropoffPassenger'
      AND object_id = OBJECT_ID(N'dbo.TaxiTrips')
)
BEGIN
    CREATE INDEX IX_TaxiTrips_PickupDropoffPassenger
        ON dbo.TaxiTrips (tpep_pickup_datetime, tpep_dropoff_datetime, passenger_count);
END
GO