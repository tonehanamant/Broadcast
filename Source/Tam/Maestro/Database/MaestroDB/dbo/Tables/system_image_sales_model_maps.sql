CREATE TABLE [dbo].[system_image_sales_model_maps] (
    [id]             INT          IDENTITY (1, 1) NOT NULL,
    [system_id]      INT          NULL,
    [image_id]       INT          NOT NULL,
    [sales_model_id] INT          NOT NULL,
    [map_set]        VARCHAR (63) NOT NULL,
    [effective_date] DATETIME     NOT NULL,
    CONSTRAINT [PK_system_image_sales_model_maps] PRIMARY KEY CLUSTERED ([id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_system_image_sales_model_maps_images] FOREIGN KEY ([image_id]) REFERENCES [dbo].[images] ([id]),
    CONSTRAINT [FK_system_image_sales_model_maps_sales_models] FOREIGN KEY ([sales_model_id]) REFERENCES [dbo].[sales_models] ([id]),
    CONSTRAINT [FK_system_image_sales_model_maps_systems] FOREIGN KEY ([system_id]) REFERENCES [dbo].[systems] ([id]),
    CONSTRAINT [IX_system_image_sales_model_maps] UNIQUE NONCLUSTERED ([system_id] ASC, [sales_model_id] ASC, [map_set] ASC, [effective_date] ASC) WITH (FILLFACTOR = 90)
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_image_sales_model_maps';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_image_sales_model_maps';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_image_sales_model_maps';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_image_sales_model_maps', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_image_sales_model_maps', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_image_sales_model_maps', @level2type = N'COLUMN', @level2name = N'system_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_image_sales_model_maps', @level2type = N'COLUMN', @level2name = N'system_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_image_sales_model_maps', @level2type = N'COLUMN', @level2name = N'image_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_image_sales_model_maps', @level2type = N'COLUMN', @level2name = N'image_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_image_sales_model_maps', @level2type = N'COLUMN', @level2name = N'sales_model_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_image_sales_model_maps', @level2type = N'COLUMN', @level2name = N'sales_model_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_image_sales_model_maps', @level2type = N'COLUMN', @level2name = N'map_set';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_image_sales_model_maps', @level2type = N'COLUMN', @level2name = N'map_set';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_image_sales_model_maps', @level2type = N'COLUMN', @level2name = N'effective_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'system_image_sales_model_maps', @level2type = N'COLUMN', @level2name = N'effective_date';

