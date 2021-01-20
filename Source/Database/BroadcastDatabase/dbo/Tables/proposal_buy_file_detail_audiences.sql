CREATE TABLE [dbo].[proposal_buy_file_detail_audiences] (
    [id]                          INT        IDENTITY (1, 1) NOT NULL,
    [proposal_buy_file_detail_id] INT        NOT NULL,
    [audience_id]                 INT        NOT NULL,
    [audience_rank]               INT        NOT NULL,
    [audience_population]         INT        NOT NULL,
    [impressions]                 FLOAT (53) NOT NULL,
    CONSTRAINT [PK_proposal_buy_file_detail_audiences] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_proposal_buy_file_detail_audiences_audiences] FOREIGN KEY ([audience_id]) REFERENCES [dbo].[audiences] ([id]),
    CONSTRAINT [FK_proposal_buy_file_detail_audiences_proposal_buy_file_details] FOREIGN KEY ([proposal_buy_file_detail_id]) REFERENCES [dbo].[proposal_buy_file_details] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_proposal_buy_file_detail_audiences_proposal_buy_file_details]
    ON [dbo].[proposal_buy_file_detail_audiences]([proposal_buy_file_detail_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_proposal_buy_file_detail_audiences_audiences]
    ON [dbo].[proposal_buy_file_detail_audiences]([audience_id] ASC);

