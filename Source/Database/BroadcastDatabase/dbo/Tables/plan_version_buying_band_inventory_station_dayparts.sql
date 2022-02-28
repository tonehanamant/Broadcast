CREATE TABLE plan_version_buying_band_inventory_station_dayparts
(
	id INT IDENTITY(1,1) NOT NULL,
	plan_version_buying_band_inventory_station_id INT NOT NULL,
	active_days INT NOT NULL,
	[hours] INT NOT NULL,
	CONSTRAINT [PK_plan_version_buying_band_inventory_station_dayparts] PRIMARY KEY CLUSTERED ([id] ASC),
	CONSTRAINT [FK_plan_version_buying_band_inventory_station_dayparts_plan_version_buying_band_inventory_station_id] FOREIGN KEY (plan_version_buying_band_inventory_station_id) REFERENCES plan_version_buying_band_inventory_stations(id) ON DELETE CASCADE
)
