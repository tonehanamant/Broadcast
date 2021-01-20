CREATE TABLE [dbo].[station_inventory_manifest_staging] (
    [id]                      INT             IDENTITY (1, 1) NOT NULL,
    [file_id]                 INT             NULL,
    [inventory_source_id]     INT             NOT NULL,
    [station]                 VARCHAR (63)    NOT NULL,
    [spots_per_week]          INT             NULL,
    [spots_per_day]           INT             NULL,
    [manifest_spot_length_id] INT             NOT NULL,
    [effective_date]          DATETIME        NOT NULL,
    [end_date]                DATETIME        NULL,
    [audience_id]             INT             NOT NULL,
    [impressions]             FLOAT (53)      NULL,
    [rating]                  FLOAT (53)      NULL,
    [audience_rate]           DECIMAL (19, 4) NOT NULL,
    [is_reference]            BIT             NOT NULL,
    [daypart_id]              INT             NOT NULL,
    [program_name]            VARCHAR (255)   NULL,
    [rate]                    DECIMAL (19, 4) NOT NULL,
    [rate_spot_length_id]     INT             NOT NULL,
    [manifest_id]             VARCHAR (63)    NOT NULL,
    CONSTRAINT [PK_station_inventory_manifest_staging] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_station_inventory_manifest_staging_audiences] FOREIGN KEY ([audience_id]) REFERENCES [dbo].[audiences] ([id]),
    CONSTRAINT [FK_station_inventory_manifest_staging_dayparts] FOREIGN KEY ([daypart_id]) REFERENCES [dbo].[dayparts] ([id]),
    CONSTRAINT [FK_station_inventory_manifest_staging_inventory_files] FOREIGN KEY ([file_id]) REFERENCES [dbo].[inventory_files] ([id]),
    CONSTRAINT [FK_station_inventory_manifest_staging_inventory_sources] FOREIGN KEY ([inventory_source_id]) REFERENCES [dbo].[inventory_sources] ([id]),
    CONSTRAINT [FK_station_inventory_manifest_staging_rate_spot_lengths] FOREIGN KEY ([rate_spot_length_id]) REFERENCES [dbo].[spot_lengths] ([id]),
    CONSTRAINT [FK_station_inventory_manifest_staging_spot_lengths] FOREIGN KEY ([manifest_spot_length_id]) REFERENCES [dbo].[spot_lengths] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_station_inventory_manifest_staging_inventory_files]
    ON [dbo].[station_inventory_manifest_staging]([file_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_station_inventory_manifest_staging_spot_lengths]
    ON [dbo].[station_inventory_manifest_staging]([manifest_spot_length_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_station_inventory_manifest_staging_rate_spot_lengths]
    ON [dbo].[station_inventory_manifest_staging]([rate_spot_length_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_station_inventory_manifest_staging_inventory_sources]
    ON [dbo].[station_inventory_manifest_staging]([inventory_source_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_station_inventory_manifest_staging_dayparts]
    ON [dbo].[station_inventory_manifest_staging]([daypart_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_station_inventory_manifest_staging_audiences]
    ON [dbo].[station_inventory_manifest_staging]([audience_id] ASC);

