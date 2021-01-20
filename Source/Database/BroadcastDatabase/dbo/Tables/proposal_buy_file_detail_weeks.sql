CREATE TABLE [dbo].[proposal_buy_file_detail_weeks] (
    [id]                          INT IDENTITY (1, 1) NOT NULL,
    [proposal_buy_file_detail_id] INT NOT NULL,
    [media_week_id]               INT NOT NULL,
    [spots]                       INT NOT NULL,
    CONSTRAINT [PK_proposal_buy_file_detail_weeks] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_proposal_buy_file_detail_weeks_media_weeks] FOREIGN KEY ([media_week_id]) REFERENCES [dbo].[media_weeks] ([id]),
    CONSTRAINT [FK_proposal_buy_file_detail_weeks_proposal_buy_file_details] FOREIGN KEY ([proposal_buy_file_detail_id]) REFERENCES [dbo].[proposal_buy_file_details] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_proposal_buy_file_detail_weeks_proposal_buy_file_details]
    ON [dbo].[proposal_buy_file_detail_weeks]([proposal_buy_file_detail_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_proposal_buy_file_detail_weeks_media_weeks]
    ON [dbo].[proposal_buy_file_detail_weeks]([media_week_id] ASC);

