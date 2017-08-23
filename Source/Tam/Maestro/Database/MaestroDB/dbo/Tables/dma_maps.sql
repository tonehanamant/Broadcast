CREATE TABLE [dbo].[dma_maps] (
    [id]             INT          IDENTITY (1, 1) NOT NULL,
    [dma_id]         INT          NOT NULL,
    [map_set]        VARCHAR (15) NOT NULL,
    [map_value]      VARCHAR (63) NOT NULL,
    [active]         BIT          NOT NULL,
    [flag]           TINYINT      CONSTRAINT [DF__dma_maps__flag__69279377] DEFAULT ((1)) NULL,
    [effective_date] DATETIME     NOT NULL,
    CONSTRAINT [PK_dma_maps] PRIMARY KEY CLUSTERED ([id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_dma_maps_dmas] FOREIGN KEY ([dma_id]) REFERENCES [dbo].[dmas] ([id])
);


GO
CREATE NONCLUSTERED INDEX [ix_dma_map_flag]
    ON [dbo].[dma_maps]([flag] ASC) WITH (FILLFACTOR = 90);


GO
CREATE TRIGGER tr_dma_maps 
	ON dbo.dma_maps 
	AFTER INSERT 
AS 

SET NOCOUNT ON 

--insert trigger event 
INSERT INTO trigger_status (id, table_name, trigger_name, trigger_start_time, trigger_flag) 
VALUES(2, 'dma_maps', 'tr_dma_maps', GETDATE(), 1)

GO
DISABLE TRIGGER [dbo].[tr_dma_maps]
    ON [dbo].[dma_maps];


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Contains multiple map sets which pair invalid DMA names with an index into our dmas table.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dma_maps';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'Designated Market Area Maps', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dma_maps';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'Use this table to lookup current mapped values.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dma_maps';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Unique Identifier', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dma_maps', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dma_maps', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'A reference to the dma record.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dma_maps', @level2type = N'COLUMN', @level2name = N'dma_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dma_maps', @level2type = N'COLUMN', @level2name = N'dma_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Logical grouping of dma map entries.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dma_maps', @level2type = N'COLUMN', @level2name = N'map_set';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dma_maps', @level2type = N'COLUMN', @level2name = N'map_set';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The invalid value being mapped.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dma_maps', @level2type = N'COLUMN', @level2name = N'map_value';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dma_maps', @level2type = N'COLUMN', @level2name = N'map_value';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dma_maps', @level2type = N'COLUMN', @level2name = N'active';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dma_maps', @level2type = N'COLUMN', @level2name = N'active';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dma_maps', @level2type = N'COLUMN', @level2name = N'flag';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dma_maps', @level2type = N'COLUMN', @level2name = N'flag';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Starting date this data is accurate.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dma_maps', @level2type = N'COLUMN', @level2name = N'effective_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'dma_maps', @level2type = N'COLUMN', @level2name = N'effective_date';

