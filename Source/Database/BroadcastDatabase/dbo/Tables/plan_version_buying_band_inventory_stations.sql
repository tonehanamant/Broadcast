CREATE TABLE [dbo].[plan_version_buying_band_inventory_stations]
(
	id INT IDENTITY(1,1) NOT NULL,
	plan_version_buying_job_id INT NOT NULL,
	posting_type_id INT NOT NULL,  
	station_id INT NOT NULL,
	impressions FLOAT NOT NULL,
	cost MONEY NOT NULL,
	manifest_weeks_count INT NOT NULL,
	CONSTRAINT [PK_plan_version_buying_band_inventory_stations] PRIMARY KEY CLUSTERED ([id] ASC),
	CONSTRAINT [FK_plan_version_buying_band_inventory_stations_plan_version_buying_job_id] FOREIGN KEY (plan_version_buying_job_id) REFERENCES plan_version_buying_job(id) ON DELETE CASCADE,
	CONSTRAINT [FK_plan_version_buying_band_inventory_stations_stations_id] FOREIGN KEY (station_id) REFERENCES stations(id) ON DELETE CASCADE
)
