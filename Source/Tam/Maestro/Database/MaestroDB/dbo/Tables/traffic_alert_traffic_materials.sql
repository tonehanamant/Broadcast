CREATE TABLE [dbo].[traffic_alert_traffic_materials] (
    [traffic_alert_id]            INT           NOT NULL,
    [traffic_material_id]         INT           NOT NULL,
    [start_date]                  DATETIME      NOT NULL,
    [end_date]                    DATETIME      NOT NULL,
    [rotation]                    INT           NOT NULL,
    [disposition_id]              INT           NULL,
    [comment]                     VARCHAR (255) NULL,
    [topography_id]               INT           NULL,
    [sort_order]                  INT           NULL,
    [reel_material_id]            INT           NULL,
    [hd_traffic_material_id]      INT           NULL,
    [created_by_usecase_id]       INT           NULL,
    [created_by_traffic_alert_id] INT           NULL,
    CONSTRAINT [PK_traffic_alert_traffic_materials] PRIMARY KEY CLUSTERED ([traffic_alert_id] ASC, [traffic_material_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_traffic_alert_traffic_materials_traffic_alerts] FOREIGN KEY ([traffic_alert_id]) REFERENCES [dbo].[traffic_alerts] ([id]) ON DELETE CASCADE
);

