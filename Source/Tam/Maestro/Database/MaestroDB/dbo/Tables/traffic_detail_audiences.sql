CREATE TABLE [dbo].[traffic_detail_audiences] (
    [traffic_detail_id] INT        NOT NULL,
    [audience_id]       INT        NOT NULL,
    [traffic_rating]    FLOAT (53) NOT NULL,
    [vpvh]              FLOAT (53) NULL,
    [proposal_rating]   FLOAT (53) NOT NULL,
    [us_universe]       FLOAT (53) NOT NULL,
    [scaling_factor]    FLOAT (53) NULL,
    [coverage_universe] FLOAT (53) NULL,
    CONSTRAINT [PK_traffic_detail_audiences] PRIMARY KEY CLUSTERED ([traffic_detail_id] ASC, [audience_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_traffic_detail_audiences_audiences] FOREIGN KEY ([audience_id]) REFERENCES [dbo].[audiences] ([id]),
    CONSTRAINT [FK_traffic_detail_audiencess_traffic_details] FOREIGN KEY ([traffic_detail_id]) REFERENCES [dbo].[traffic_details] ([id])
);




GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_detail_audiences';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_detail_audiences';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_detail_audiences';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_detail_audiences', @level2type = N'COLUMN', @level2name = N'traffic_detail_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_detail_audiences', @level2type = N'COLUMN', @level2name = N'traffic_detail_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_detail_audiences', @level2type = N'COLUMN', @level2name = N'audience_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_detail_audiences', @level2type = N'COLUMN', @level2name = N'audience_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_detail_audiences', @level2type = N'COLUMN', @level2name = N'traffic_rating';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_detail_audiences', @level2type = N'COLUMN', @level2name = N'traffic_rating';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_detail_audiences', @level2type = N'COLUMN', @level2name = N'vpvh';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_detail_audiences', @level2type = N'COLUMN', @level2name = N'vpvh';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_detail_audiences', @level2type = N'COLUMN', @level2name = N'proposal_rating';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_detail_audiences', @level2type = N'COLUMN', @level2name = N'proposal_rating';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_detail_audiences', @level2type = N'COLUMN', @level2name = N'us_universe';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'traffic_detail_audiences', @level2type = N'COLUMN', @level2name = N'us_universe';

