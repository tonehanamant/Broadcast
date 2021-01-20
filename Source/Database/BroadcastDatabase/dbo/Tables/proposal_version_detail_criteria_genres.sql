CREATE TABLE [dbo].[proposal_version_detail_criteria_genres] (
    [id]                         INT     IDENTITY (1, 1) NOT NULL,
    [proposal_version_detail_id] INT     NOT NULL,
    [contain_type]               TINYINT NOT NULL,
    [genre_id]                   INT     NOT NULL,
    CONSTRAINT [PK_proposal_version_detail_criteria_genres] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_proposal_version_detail_criteria_genres_genres] FOREIGN KEY ([genre_id]) REFERENCES [dbo].[genres] ([id]) ON DELETE CASCADE,
    CONSTRAINT [FK_proposal_version_detail_criteria_genres_proposal_version_details] FOREIGN KEY ([proposal_version_detail_id]) REFERENCES [dbo].[proposal_version_details] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_proposal_version_detail_criteria_genres_genres]
    ON [dbo].[proposal_version_detail_criteria_genres]([genre_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_proposal_version_detail_criteria_genres_proposal_version_details]
    ON [dbo].[proposal_version_detail_criteria_genres]([proposal_version_detail_id] ASC);

