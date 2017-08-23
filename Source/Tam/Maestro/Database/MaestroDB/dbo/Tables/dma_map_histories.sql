CREATE TABLE [dbo].[dma_map_histories] (
    [dma_map_id] INT          NOT NULL,
    [start_date] DATETIME     NOT NULL,
    [dma_id]     INT          NOT NULL,
    [map_set]    VARCHAR (15) NOT NULL,
    [map_value]  VARCHAR (63) NOT NULL,
    [active]     BIT          NOT NULL,
    [flag]       TINYINT      CONSTRAINT [DF_dma_map_histories_flag] DEFAULT ((1)) NULL,
    [end_date]   DATETIME     NOT NULL,
    CONSTRAINT [PK_dma_map_histories] PRIMARY KEY CLUSTERED ([dma_map_id] ASC, [start_date] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_dma_map_histories_dma_maps] FOREIGN KEY ([dma_map_id]) REFERENCES [dbo].[dma_maps] ([id]),
    CONSTRAINT [FK_dma_map_histories_dmas] FOREIGN KEY ([dma_id]) REFERENCES [dbo].[dmas] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dma_map_histories', @level2type = N'COLUMN', @level2name = N'dma_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dma_map_histories', @level2type = N'COLUMN', @level2name = N'dma_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dma_map_histories', @level2type = N'COLUMN', @level2name = N'map_set';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dma_map_histories', @level2type = N'COLUMN', @level2name = N'map_set';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dma_map_histories', @level2type = N'COLUMN', @level2name = N'map_value';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dma_map_histories', @level2type = N'COLUMN', @level2name = N'map_value';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dma_map_histories', @level2type = N'COLUMN', @level2name = N'active';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dma_map_histories', @level2type = N'COLUMN', @level2name = N'active';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dma_map_histories', @level2type = N'COLUMN', @level2name = N'flag';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dma_map_histories', @level2type = N'COLUMN', @level2name = N'flag';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Ending date this data was accurate.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dma_map_histories', @level2type = N'COLUMN', @level2name = N'end_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dma_map_histories', @level2type = N'COLUMN', @level2name = N'end_date';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Contains multiple map sets which pair invalid DMA names with an index into our dmas table.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dma_map_histories';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'Designated Markert Area Map History', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dma_map_histories';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'Use this table to lookup what a map value was for a specific period of time.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dma_map_histories';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'A reference to the dma record.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dma_map_histories', @level2type = N'COLUMN', @level2name = N'dma_map_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dma_map_histories', @level2type = N'COLUMN', @level2name = N'dma_map_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Starting date this data was accurate.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dma_map_histories', @level2type = N'COLUMN', @level2name = N'start_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dma_map_histories', @level2type = N'COLUMN', @level2name = N'start_date';

