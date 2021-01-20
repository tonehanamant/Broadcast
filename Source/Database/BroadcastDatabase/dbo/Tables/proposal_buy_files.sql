CREATE TABLE [dbo].[proposal_buy_files] (
    [id]                         INT           IDENTITY (1, 1) NOT NULL,
    [file_name]                  VARCHAR (255) NOT NULL,
    [file_hash]                  VARCHAR (63)  NOT NULL,
    [estimate_id]                INT           NULL,
    [proposal_version_detail_id] INT           NOT NULL,
    [start_date]                 DATETIME      NOT NULL,
    [end_date]                   DATETIME      NOT NULL,
    [created_by]                 VARCHAR (63)  NOT NULL,
    [created_date]               DATETIME      NOT NULL,
    CONSTRAINT [PK_proposal_buy_files] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_proposal_buy_files_proposal_version_details] FOREIGN KEY ([proposal_version_detail_id]) REFERENCES [dbo].[proposal_version_details] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_proposal_buy_files_proposal_version_details]
    ON [dbo].[proposal_buy_files]([proposal_version_detail_id] ASC);

