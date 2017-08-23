CREATE TABLE [dbo].[traffic_alert_material_maps] (
    [id]                  INT IDENTITY (1, 1) NOT NULL,
    [traffic_alert_id]    INT NOT NULL,
    [traffic_material_id] INT NOT NULL,
    [rank]                INT NULL,
    CONSTRAINT [PK_traffic_alert_material_maps_1] PRIMARY KEY CLUSTERED ([id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_traffic_alert_material_maps_traffic_alerts] FOREIGN KEY ([traffic_alert_id]) REFERENCES [dbo].[traffic_alerts] ([id]),
    CONSTRAINT [FK_traffic_alert_material_maps_traffic_materials] FOREIGN KEY ([traffic_material_id]) REFERENCES [dbo].[traffic_materials] ([id]),
    CONSTRAINT [IX_traffic_alert_material_maps] UNIQUE NONCLUSTERED ([traffic_alert_id] ASC, [traffic_material_id] ASC) WITH (FILLFACTOR = 90)
);




GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_alert_material_maps';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_alert_material_maps';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_alert_material_maps';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_alert_material_maps', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_alert_material_maps', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_alert_material_maps', @level2type = N'COLUMN', @level2name = N'traffic_alert_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_alert_material_maps', @level2type = N'COLUMN', @level2name = N'traffic_alert_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_alert_material_maps', @level2type = N'COLUMN', @level2name = N'traffic_material_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_alert_material_maps', @level2type = N'COLUMN', @level2name = N'traffic_material_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_alert_material_maps', @level2type = N'COLUMN', @level2name = N'rank';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_alert_material_maps', @level2type = N'COLUMN', @level2name = N'rank';


GO
CREATE NONCLUSTERED INDEX [IX_traffic_alert_material_maps_traffic_material_id]
    ON [dbo].[traffic_alert_material_maps]([traffic_material_id] ASC)
    INCLUDE([id]);

