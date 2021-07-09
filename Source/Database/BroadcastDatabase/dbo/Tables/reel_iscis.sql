CREATE TABLE [dbo].[reel_iscis]
(
	[id] INT NOT NULL PRIMARY KEY IDENTITY (1, 1), 
	[isci] VARCHAR(50) NOT NULL,
	[spot_length_id] INT NOT NULL,
	[active_start_date] DATETIME2 NOT NULL,
	[active_end_date] DATETIME2 NOT NULL,
	[ingested_at] DATETIME2 NOT NULL,
	CONSTRAINT [FK_reel_iscis_spot_lengths] FOREIGN KEY ([spot_length_id]) REFERENCES [dbo].[spot_lengths]([ID])
);
