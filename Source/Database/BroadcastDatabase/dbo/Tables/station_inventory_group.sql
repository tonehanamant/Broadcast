CREATE TABLE [dbo].[station_inventory_group] (
    [id]                  INT          IDENTITY (1, 1) NOT NULL,
    [name]                VARCHAR (50) NOT NULL,
    [slot_number]         TINYINT      NOT NULL,
    [inventory_source_id] INT          NOT NULL,
    CONSTRAINT [PK_station_inventory_group] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_station_inventory_group_inventory_sources] FOREIGN KEY ([inventory_source_id]) REFERENCES [dbo].[inventory_sources] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_station_inventory_group_inventory_sources]
    ON [dbo].[station_inventory_group]([inventory_source_id] ASC);

