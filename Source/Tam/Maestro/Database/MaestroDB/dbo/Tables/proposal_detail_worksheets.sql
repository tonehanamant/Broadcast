CREATE TABLE [dbo].[proposal_detail_worksheets] (
    [proposal_detail_id] INT NOT NULL,
    [media_week_id]      INT NOT NULL,
    [units]              INT NOT NULL,
    CONSTRAINT [PK_proposal_detail_worksheets] PRIMARY KEY CLUSTERED ([proposal_detail_id] ASC, [media_week_id] ASC),
    CONSTRAINT [FK_proposal_detail_worksheets_media_weeks] FOREIGN KEY ([media_week_id]) REFERENCES [dbo].[media_weeks] ([id]),
    CONSTRAINT [FK_proposal_detail_worksheets_proposal_details] FOREIGN KEY ([proposal_detail_id]) REFERENCES [dbo].[proposal_details] ([id])
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_detail_worksheets';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_detail_worksheets';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_detail_worksheets';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_detail_worksheets', @level2type = N'COLUMN', @level2name = N'proposal_detail_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_detail_worksheets', @level2type = N'COLUMN', @level2name = N'proposal_detail_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_detail_worksheets', @level2type = N'COLUMN', @level2name = N'media_week_id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_detail_worksheets', @level2type = N'COLUMN', @level2name = N'media_week_id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_detail_worksheets', @level2type = N'COLUMN', @level2name = N'units';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'proposal_detail_worksheets', @level2type = N'COLUMN', @level2name = N'units';

