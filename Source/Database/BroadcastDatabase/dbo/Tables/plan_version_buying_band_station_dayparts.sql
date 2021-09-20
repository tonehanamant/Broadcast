CREATE TABLE [dbo].[plan_version_buying_band_station_dayparts](
	[id] int NOT NULL PRIMARY KEY IDENTITY(1,1),
	[plan_version_buying_band_station_id] [int] NOT NULL,
	[active_days] [int] NOT NULL,
	[hours] [int] NOT NULL,
	CONSTRAINT [FK_plan_version_buying_band_station_dayparts_plan_version_buying_band_stations] FOREIGN KEY([plan_version_buying_band_station_id]) REFERENCES [dbo].[plan_version_buying_band_stations] ([id])
)