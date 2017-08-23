CREATE TABLE [dbo].[proposal_detail_audiences] (
    [proposal_detail_id] INT        NOT NULL,
    [audience_id]        INT        NOT NULL,
    [rating]             FLOAT (53) NOT NULL,
    [vpvh]               FLOAT (53) NULL,
    [coverage_delivery]  FLOAT (53) NULL,
    [us_delivery]        FLOAT (53) NULL,
    [us_universe]        FLOAT (53) NULL,
    CONSTRAINT [PK_proposal_detail_audiences] PRIMARY KEY CLUSTERED ([proposal_detail_id] ASC, [audience_id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_proposal_detail_audiences_audiences] FOREIGN KEY ([audience_id]) REFERENCES [dbo].[audiences] ([id]),
    CONSTRAINT [FK_proposal_detail_audiencess_proposal_details] FOREIGN KEY ([proposal_detail_id]) REFERENCES [dbo].[proposal_details] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_detail_audiences';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_detail_audiences';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_detail_audiences';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_detail_audiences', @level2type = N'COLUMN', @level2name = N'proposal_detail_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_detail_audiences', @level2type = N'COLUMN', @level2name = N'proposal_detail_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_detail_audiences', @level2type = N'COLUMN', @level2name = N'audience_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_detail_audiences', @level2type = N'COLUMN', @level2name = N'audience_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_detail_audiences', @level2type = N'COLUMN', @level2name = N'rating';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_detail_audiences', @level2type = N'COLUMN', @level2name = N'rating';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_detail_audiences', @level2type = N'COLUMN', @level2name = N'vpvh';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_detail_audiences', @level2type = N'COLUMN', @level2name = N'vpvh';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_detail_audiences', @level2type = N'COLUMN', @level2name = N'coverage_delivery';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_detail_audiences', @level2type = N'COLUMN', @level2name = N'coverage_delivery';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_detail_audiences', @level2type = N'COLUMN', @level2name = N'us_delivery';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_detail_audiences', @level2type = N'COLUMN', @level2name = N'us_delivery';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_detail_audiences', @level2type = N'COLUMN', @level2name = N'us_universe';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_detail_audiences', @level2type = N'COLUMN', @level2name = N'us_universe';

