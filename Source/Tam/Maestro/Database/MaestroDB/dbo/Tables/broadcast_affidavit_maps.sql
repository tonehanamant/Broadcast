CREATE TABLE [dbo].[broadcast_affidavit_maps] (
    [broadcast_affidavit_map_type_id] INT            NOT NULL,
    [affidavit_value]                 NVARCHAR (255) NOT NULL,
    [broadcast_value]                 NVARCHAR (255) NOT NULL,
    CONSTRAINT [PK_broadcast_affidavit_maps] PRIMARY KEY CLUSTERED ([broadcast_affidavit_map_type_id] ASC, [affidavit_value] ASC, [broadcast_value] ASC),
    CONSTRAINT [FK_broadcast_affidavit_maps_broadcast_affidavit_map_types] FOREIGN KEY ([broadcast_affidavit_map_type_id]) REFERENCES [dbo].[broadcast_affidavit_map_types] ([id])
);

