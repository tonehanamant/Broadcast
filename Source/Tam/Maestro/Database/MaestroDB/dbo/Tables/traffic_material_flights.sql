CREATE TABLE [dbo].[traffic_material_flights] (
    [id]                  INT      IDENTITY (1, 1) NOT NULL,
    [traffic_material_id] INT      NOT NULL,
    [start_date]          DATETIME NOT NULL,
    [end_date]            DATETIME NOT NULL,
    [selected]            BIT      NOT NULL,
    [media_week_id]       INT      NOT NULL,
    CONSTRAINT [PK_traffic_material_flights_1] PRIMARY KEY CLUSTERED ([id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_traffic_material_flights_media_week_id_media_weeks] FOREIGN KEY ([media_week_id]) REFERENCES [dbo].[media_weeks] ([id]),
    CONSTRAINT [FK_traffic_material_flights_traffic_materials] FOREIGN KEY ([traffic_material_id]) REFERENCES [dbo].[traffic_materials] ([id]),
    CONSTRAINT [IX_traffic_material_flights] UNIQUE NONCLUSTERED ([traffic_material_id] ASC, [start_date] ASC) WITH (FILLFACTOR = 90)
);




GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_material_flights';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_material_flights';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_material_flights';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_material_flights', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_material_flights', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_material_flights', @level2type = N'COLUMN', @level2name = N'traffic_material_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_material_flights', @level2type = N'COLUMN', @level2name = N'traffic_material_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_material_flights', @level2type = N'COLUMN', @level2name = N'start_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_material_flights', @level2type = N'COLUMN', @level2name = N'start_date';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_material_flights', @level2type = N'COLUMN', @level2name = N'end_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_material_flights', @level2type = N'COLUMN', @level2name = N'end_date';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_material_flights', @level2type = N'COLUMN', @level2name = N'selected';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_material_flights', @level2type = N'COLUMN', @level2name = N'selected';

