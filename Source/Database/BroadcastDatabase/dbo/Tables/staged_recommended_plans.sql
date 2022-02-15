CREATE TABLE staged_recommended_plans
(
	id INT IDENTITY(1,1) NOT NULL,
	spot_unique_hash_external VARCHAR(255) NOT NULL,
	execution_id_external VARCHAR(100) NOT NULL,
	ambiguity_code INT NOT NULL,
	estimate_id INT NOT NULL,
	inventory_source VARCHAR(100) NOT NULL,
	house_isci VARCHAR(100) NOT NULL,
	client_isci VARCHAR(100) NOT NULL,
	client_spot_length INT NOT NULL,
	broadcast_aired_date DATETIME2 NOT NULL,
	aired_time INT NOT NULL,
	station_legacy_call_letters VARCHAR(30) NULL,
	affiliate VARCHAR(30) NULL,
	market_code INT NULL,
	market_rank INT NULL,	
	[program_name] VARCHAR(500) NOT NULL,
	program_genre VARCHAR(127) NOT NULL,
	ingested_by VARCHAR(100) NOT NULL,
	ingested_at DATETIME NOT NULL,
	CONSTRAINT [PK_staged_recommended_plans] PRIMARY KEY CLUSTERED ([id] ASC)
);