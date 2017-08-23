CREATE TABLE [dbo].[sales_model_traffic_vigs] (
    [id]             INT          IDENTITY (1, 1) NOT NULL,
    [sales_model_id] INT          NULL,
    [map_name]       VARCHAR (63) NOT NULL,
    [value]          FLOAT (53)   NOT NULL,
    CONSTRAINT [PK_sales_model_traffic_vigs_3] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_sales_model_traffic_vigs_sales_models] FOREIGN KEY ([sales_model_id]) REFERENCES [dbo].[sales_models] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_model_traffic_vigs';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_model_traffic_vigs';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_model_traffic_vigs';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_model_traffic_vigs', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_model_traffic_vigs', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_model_traffic_vigs', @level2type = N'COLUMN', @level2name = N'sales_model_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_model_traffic_vigs', @level2type = N'COLUMN', @level2name = N'sales_model_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_model_traffic_vigs', @level2type = N'COLUMN', @level2name = N'map_name';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_model_traffic_vigs', @level2type = N'COLUMN', @level2name = N'map_name';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_model_traffic_vigs', @level2type = N'COLUMN', @level2name = N'value';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'sales_model_traffic_vigs', @level2type = N'COLUMN', @level2name = N'value';

