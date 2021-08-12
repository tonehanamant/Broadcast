CREATE TABLE [dbo].[plan_version_buying_result_spot_stations]
(
		[id] INT NOT NULL primary key IDENTITY, 
		[plan_version_buying_result_id] int not null,
		[program_name] varchar(255) NOT NULL,
		[genre] varchar(500) NOT NULL,
		[station] varchar(15) NOT NULL,
		[impressions] FLOAT NOT NULL,
		[spots] INT NOT NULL, 
		[budget] decimal NOT NULL,
		CONSTRAINT [FK_plan_version_buying_result_spot_stations_plan_version_buying_results] FOREIGN KEY ([plan_version_buying_result_id]) REFERENCES [dbo].[plan_version_buying_results]
		
)
