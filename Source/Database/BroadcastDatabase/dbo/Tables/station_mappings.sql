CREATE TABLE [dbo].[station_mappings] (
    [id]                  INT          IDENTITY (1, 1) NOT NULL,
    [mapped_call_letters] VARCHAR (15) NOT NULL,
    [created_date]        DATETIME     NOT NULL,
    [created_by]          VARCHAR (63) NOT NULL,
    [station_id]          INT          NOT NULL,
    [map_set]             INT          NOT NULL,
    CONSTRAINT [PK_station_mappings] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_station_mappings_stations] FOREIGN KEY ([station_id]) REFERENCES [dbo].[stations] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_station_mappings_stations]
    ON [dbo].[station_mappings]([station_id] ASC);

