CREATE TABLE [dbo].[traffic_detail_topographies_orig] (
    [topography_id]          INT        NOT NULL,
    [spots]                  FLOAT (53) NOT NULL,
    [universe]               FLOAT (53) NULL,
    [rate]                   MONEY      NULL,
    [display_daypart]        INT        NULL,
    [traffic_detail_week_id] INT        NOT NULL,
    CONSTRAINT [PK_traffic_detail_topographies_orig] PRIMARY KEY CLUSTERED ([topography_id] ASC, [traffic_detail_week_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_traffic_detail_topographies_dayparts_orig] FOREIGN KEY ([display_daypart]) REFERENCES [dbo].[dayparts] ([id]),
    CONSTRAINT [FK_traffic_detail_topographies_topographies_orig] FOREIGN KEY ([topography_id]) REFERENCES [dbo].[topographies] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_traffic_detail_topographies_daypart_id_orig]
    ON [dbo].[traffic_detail_topographies_orig]([display_daypart] ASC)
    INCLUDE([traffic_detail_week_id]) WITH (FILLFACTOR = 90);


GO
CREATE NONCLUSTERED INDEX [IX_traffic_detail_topographies_display_daypart_traffic_detail_week_id_orig]
    ON [dbo].[traffic_detail_topographies_orig]([display_daypart] ASC, [traffic_detail_week_id] ASC) WITH (FILLFACTOR = 90);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_traffic_detail_topographies_orig]
    ON [dbo].[traffic_detail_topographies_orig]([traffic_detail_week_id] ASC, [topography_id] ASC, [display_daypart] ASC)
    INCLUDE([spots], [universe], [rate]) WITH (FILLFACTOR = 90);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_detail_topographies_orig';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_detail_topographies_orig';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_detail_topographies_orig';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_detail_topographies_orig', @level2type = N'COLUMN', @level2name = N'topography_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_detail_topographies_orig', @level2type = N'COLUMN', @level2name = N'topography_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_detail_topographies_orig', @level2type = N'COLUMN', @level2name = N'spots';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_detail_topographies_orig', @level2type = N'COLUMN', @level2name = N'spots';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_detail_topographies_orig', @level2type = N'COLUMN', @level2name = N'universe';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_detail_topographies_orig', @level2type = N'COLUMN', @level2name = N'universe';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_detail_topographies_orig', @level2type = N'COLUMN', @level2name = N'rate';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_detail_topographies_orig', @level2type = N'COLUMN', @level2name = N'rate';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_detail_topographies_orig', @level2type = N'COLUMN', @level2name = N'display_daypart';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_detail_topographies_orig', @level2type = N'COLUMN', @level2name = N'display_daypart';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_detail_topographies_orig', @level2type = N'COLUMN', @level2name = N'traffic_detail_week_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_detail_topographies_orig', @level2type = N'COLUMN', @level2name = N'traffic_detail_week_id';

