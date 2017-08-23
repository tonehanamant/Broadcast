CREATE TABLE [dbo].[zones] (
    [id]                            INT          IDENTITY (1, 1) NOT NULL,
    [code]                          VARCHAR (15) NOT NULL,
    [name]                          VARCHAR (63) NOT NULL,
    [type]                          VARCHAR (63) CONSTRAINT [DF_zones_type] DEFAULT ('N/A') NOT NULL,
    [primary]                       BIT          NOT NULL,
    [traffic]                       BIT          NOT NULL,
    [dma]                           BIT          NOT NULL,
    [flag]                          TINYINT      CONSTRAINT [DF__zones__flag__092A4EB5] DEFAULT ((1)) NULL,
    [active]                        BIT          NOT NULL,
    [effective_date]                DATETIME     NOT NULL,
    [time_zone_id]                  INT          NULL,
    [observe_daylight_savings_time] BIT          DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_zones] PRIMARY KEY CLUSTERED ([id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_zones_time_zones] FOREIGN KEY ([time_zone_id]) REFERENCES [dbo].[time_zones] ([id])
);




GO
CREATE NONCLUSTERED INDEX [IND_zones_code]
    ON [dbo].[zones]([code] ASC) WITH (FILLFACTOR = 90);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_zones_code]
    ON [dbo].[zones]([code] ASC) WITH (FILLFACTOR = 90);


GO
CREATE STATISTICS [_dta_stat_1140915136_1_5]
    ON [dbo].[zones]([id], [primary]);


GO
CREATE STATISTICS [_dta_stat_1140915136_9_1]
    ON [dbo].[zones]([active], [id]);


GO
CREATE STATISTICS [_dta_stat_1140915136_5_9_1]
    ON [dbo].[zones]([primary], [active], [id]);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zones';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'Zones', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zones';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zones';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zones', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zones', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zones', @level2type = N'COLUMN', @level2name = N'code';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zones', @level2type = N'COLUMN', @level2name = N'code';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zones', @level2type = N'COLUMN', @level2name = N'name';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zones', @level2type = N'COLUMN', @level2name = N'name';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zones', @level2type = N'COLUMN', @level2name = N'type';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zones', @level2type = N'COLUMN', @level2name = N'type';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zones', @level2type = N'COLUMN', @level2name = N'primary';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zones', @level2type = N'COLUMN', @level2name = N'primary';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zones', @level2type = N'COLUMN', @level2name = N'traffic';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zones', @level2type = N'COLUMN', @level2name = N'traffic';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zones', @level2type = N'COLUMN', @level2name = N'dma';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zones', @level2type = N'COLUMN', @level2name = N'dma';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zones', @level2type = N'COLUMN', @level2name = N'flag';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zones', @level2type = N'COLUMN', @level2name = N'flag';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zones', @level2type = N'COLUMN', @level2name = N'active';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zones', @level2type = N'COLUMN', @level2name = N'active';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zones', @level2type = N'COLUMN', @level2name = N'effective_date';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'zones', @level2type = N'COLUMN', @level2name = N'effective_date';

