CREATE TABLE [dbo].[plan_version_buying_band_stations](
		[id] int NOT NULL PRIMARY KEY IDENTITY(1,1),
		[plan_version_buying_result_id] int NOT NULL,
		[station_id] int NOT NULL,
		[impressions] float NOT NULL,
		[cost] money NOT NULL,
		[manifest_weeks_count] int NOT NULL,
		CONSTRAINT [FK_plan_version_buying_band_stations_plan_version_buying_results] FOREIGN KEY([plan_version_buying_result_id]) REFERENCES [dbo].[plan_version_buying_results] ([id]),
		CONSTRAINT [FK_plan_version_buying_band_stations_stations] FOREIGN KEY([station_id]) REFERENCES [dbo].[stations] ([id])
)