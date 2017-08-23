CREATE TABLE [dbo].[release_cpmlink] (
    [id]               INT        IDENTITY (1, 1) NOT NULL,
    [traffic_id]       INT        NOT NULL,
    [proposal_id]      INT        NOT NULL,
    [weighting_factor] FLOAT (53) CONSTRAINT [DF_release_cpmlink_weighting_factor] DEFAULT ((0.0)) NOT NULL,
    CONSTRAINT [PK_release_cpmlink] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_release_cpmlink_proposals] FOREIGN KEY ([proposal_id]) REFERENCES [dbo].[proposals] ([id]),
    CONSTRAINT [FK_release_cpmlink_traffic] FOREIGN KEY ([traffic_id]) REFERENCES [dbo].[traffic] ([id]),
    CONSTRAINT [IX_release_cpmlink] UNIQUE NONCLUSTERED ([proposal_id] ASC, [traffic_id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_release_cpmlink_traffic_id]
    ON [dbo].[release_cpmlink]([traffic_id] ASC);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'release_cpmlink';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'release_cpmlink';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'release_cpmlink';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'release_cpmlink', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'release_cpmlink', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'release_cpmlink', @level2type = N'COLUMN', @level2name = N'traffic_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'release_cpmlink', @level2type = N'COLUMN', @level2name = N'traffic_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'release_cpmlink', @level2type = N'COLUMN', @level2name = N'proposal_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'release_cpmlink', @level2type = N'COLUMN', @level2name = N'proposal_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'release_cpmlink', @level2type = N'COLUMN', @level2name = N'weighting_factor';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'release_cpmlink', @level2type = N'COLUMN', @level2name = N'weighting_factor';

