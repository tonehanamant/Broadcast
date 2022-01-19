CREATE TABLE staged_unposted_no_reel_roster
(
	id INT IDENTITY(1,1),
	house_isci VARCHAR(50) NOT NULL,
	spot_count INT NOT NULL,	
	program_air_time DATETIME2 NOT NULL,
	estimate_id BIGINT NOT NULL,
	ingested_by VARCHAR(100) NOT NULL,
	ingested_at DATETIME NOT NULL,
	CONSTRAINT [PK_staged_unposted_no_reel_roster] PRIMARY KEY CLUSTERED ([id] ASC),
);
