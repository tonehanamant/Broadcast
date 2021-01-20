CREATE TABLE [dbo].[station_inventory_manifest_dayparts] (
    [daypart_id]                    INT           NOT NULL,
    [station_inventory_manifest_id] INT           NOT NULL,
    [id]                            INT           IDENTITY (1, 1) NOT NULL,
    [program_name]                  VARCHAR (255) NULL,
    [primary_program_id]            INT           NULL,
    [standard_daypart_id]           INT           NULL,
    CONSTRAINT [PK_station_inventory_manifest_dayparts] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_station_inventory_manifest_dayparts_dayparts] FOREIGN KEY ([daypart_id]) REFERENCES [dbo].[dayparts] ([id]),
    CONSTRAINT [FK_station_inventory_manifest_dayparts_standard_dayparts] FOREIGN KEY ([standard_daypart_id]) REFERENCES [dbo].[standard_dayparts] ([id]),
    CONSTRAINT [FK_station_inventory_manifest_dayparts_station_inventory_manifest] FOREIGN KEY ([station_inventory_manifest_id]) REFERENCES [dbo].[station_inventory_manifest] ([id]) ON DELETE CASCADE,
    CONSTRAINT [FK_station_inventory_manifest_dayparts_station_inventory_manifest_daypart_programs] FOREIGN KEY ([primary_program_id]) REFERENCES [dbo].[station_inventory_manifest_daypart_programs] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_station_inventory_manifest_dayparts_standard_dayparts]
    ON [dbo].[station_inventory_manifest_dayparts]([standard_daypart_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_station_inventory_manifest_dayparts_station_inventory_manifest_daypart_programs]
    ON [dbo].[station_inventory_manifest_dayparts]([primary_program_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_station_inventory_manifest_dayparts_station_inventory_manifest]
    ON [dbo].[station_inventory_manifest_dayparts]([station_inventory_manifest_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_station_inventory_manifest_dayparts_dayparts]
    ON [dbo].[station_inventory_manifest_dayparts]([daypart_id] ASC);

