CREATE TABLE [dbo].[tam_post_zones] (
    [tam_post_proposal_id] INT NOT NULL,
    [zone_id]              INT NOT NULL,
    [total_spots]          INT NOT NULL,
    CONSTRAINT [PK_tam_post_zones] PRIMARY KEY CLUSTERED ([tam_post_proposal_id] ASC, [zone_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_tam_post_zones_tam_post_proposals] FOREIGN KEY ([tam_post_proposal_id]) REFERENCES [dbo].[tam_post_proposals] ([id]),
    CONSTRAINT [FK_tam_post_zones_zones] FOREIGN KEY ([zone_id]) REFERENCES [dbo].[zones] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_tam_post_zones]
    ON [dbo].[tam_post_zones]([tam_post_proposal_id] ASC)
    INCLUDE([zone_id]) WITH (FILLFACTOR = 90);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_zones';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_zones';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_zones';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_zones', @level2type = N'COLUMN', @level2name = N'tam_post_proposal_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_zones', @level2type = N'COLUMN', @level2name = N'tam_post_proposal_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_zones', @level2type = N'COLUMN', @level2name = N'zone_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_zones', @level2type = N'COLUMN', @level2name = N'zone_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_zones', @level2type = N'COLUMN', @level2name = N'total_spots';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tam_post_zones', @level2type = N'COLUMN', @level2name = N'total_spots';

