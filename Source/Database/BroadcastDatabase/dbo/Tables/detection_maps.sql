CREATE TABLE [dbo].[detection_maps] (
    [detection_map_type_id] INT            NOT NULL,
    [detection_value]       NVARCHAR (255) NOT NULL,
    [schedule_value]        NVARCHAR (255) NOT NULL,
    CONSTRAINT [PK_detection_maps] PRIMARY KEY CLUSTERED ([detection_map_type_id] ASC, [detection_value] ASC, [schedule_value] ASC),
    CONSTRAINT [FK_detection_maps_detection_map_types] FOREIGN KEY ([detection_map_type_id]) REFERENCES [dbo].[detection_map_types] ([id])
);

