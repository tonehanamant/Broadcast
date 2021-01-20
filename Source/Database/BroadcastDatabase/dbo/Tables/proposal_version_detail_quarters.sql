CREATE TABLE [dbo].[proposal_version_detail_quarters] (
    [id]                         INT             IDENTITY (1, 1) NOT NULL,
    [proposal_version_detail_id] INT             NOT NULL,
    [quarter]                    TINYINT         NOT NULL,
    [year]                       INT             NOT NULL,
    [cpm]                        DECIMAL (19, 4) NOT NULL,
    [impressions_goal]           FLOAT (53)      NOT NULL,
    CONSTRAINT [PK_proposal_version_detail_quarters] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_proposal_version_detail_quarters_proposal_version_details] FOREIGN KEY ([proposal_version_detail_id]) REFERENCES [dbo].[proposal_version_details] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_proposal_version_detail_quarters_proposal_version_details]
    ON [dbo].[proposal_version_detail_quarters]([proposal_version_detail_id] ASC);

