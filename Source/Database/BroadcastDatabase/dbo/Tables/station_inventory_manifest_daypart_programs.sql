CREATE TABLE [dbo].[station_inventory_manifest_daypart_programs] (
    [id]                                    INT           IDENTITY (1, 1) NOT NULL,
    [station_inventory_manifest_daypart_id] INT           NOT NULL,
    [name]                                  VARCHAR (255) NOT NULL,
    [show_type]                             VARCHAR (30)  NOT NULL,
    [source_genre_id]                       INT           NOT NULL,
    [start_date]                            DATETIME      NULL,
    [end_date]                              DATETIME      NULL,
    [start_time]                            INT           NOT NULL,
    [end_time]                              INT           NOT NULL,
    [created_date]                          DATETIME      NOT NULL,
    [program_source_id]                     INT           NOT NULL,
    [maestro_genre_id]                      INT           NOT NULL,
    CONSTRAINT [PK_station_inventory_manifest_daypart_programs] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_station_inventory_manifest_daypart_programs_genres] FOREIGN KEY ([source_genre_id]) REFERENCES [dbo].[genres] ([id]),
    CONSTRAINT [FK_station_inventory_manifest_daypart_programs_genres_maestro] FOREIGN KEY ([maestro_genre_id]) REFERENCES [dbo].[genres] ([id]),
    CONSTRAINT [FK_station_inventory_manifest_daypart_programs_program_sources] FOREIGN KEY ([program_source_id]) REFERENCES [dbo].[program_sources] ([id]),
    CONSTRAINT [FK_station_inventory_manifest_daypart_programs_station_inventory_manifest_dayparts] FOREIGN KEY ([station_inventory_manifest_daypart_id]) REFERENCES [dbo].[station_inventory_manifest_dayparts] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_station_inventory_manifest_daypart_programs_station_inventory_manifest_dayparts]
    ON [dbo].[station_inventory_manifest_daypart_programs]([station_inventory_manifest_daypart_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_station_inventory_manifest_daypart_programs_program_sources]
    ON [dbo].[station_inventory_manifest_daypart_programs]([program_source_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_station_inventory_manifest_daypart_programs_genres_maestro]
    ON [dbo].[station_inventory_manifest_daypart_programs]([maestro_genre_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_station_inventory_manifest_daypart_programs_genres]
    ON [dbo].[station_inventory_manifest_daypart_programs]([source_genre_id] ASC);

