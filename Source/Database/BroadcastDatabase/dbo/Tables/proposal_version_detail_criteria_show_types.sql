CREATE TABLE [dbo].[proposal_version_detail_criteria_show_types] (
    [id]                         INT     IDENTITY (1, 1) NOT NULL,
    [proposal_version_detail_id] INT     NOT NULL,
    [contain_type]               TINYINT NOT NULL,
    [show_type_id]               INT     NOT NULL,
    CONSTRAINT [PK_proposal_version_detail_criteria_show_types] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_proposal_version_detail_criteria_show_types_show_types] FOREIGN KEY ([show_type_id]) REFERENCES [dbo].[show_types] ([id]) ON DELETE CASCADE,
    CONSTRAINT [FK_proposal_version_details_show_types] FOREIGN KEY ([proposal_version_detail_id]) REFERENCES [dbo].[proposal_version_details] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_proposal_version_details_show_types]
    ON [dbo].[proposal_version_detail_criteria_show_types]([proposal_version_detail_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_proposal_version_detail_criteria_show_types_show_types]
    ON [dbo].[proposal_version_detail_criteria_show_types]([show_type_id] ASC);

