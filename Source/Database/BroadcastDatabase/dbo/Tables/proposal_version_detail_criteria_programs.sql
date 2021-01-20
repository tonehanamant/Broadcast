CREATE TABLE [dbo].[proposal_version_detail_criteria_programs] (
    [id]                         INT            IDENTITY (1, 1) NOT NULL,
    [proposal_version_detail_id] INT            NOT NULL,
    [contain_type]               TINYINT        NOT NULL,
    [program_name]               NVARCHAR (100) NOT NULL,
    [program_name_id]            INT            NOT NULL,
    CONSTRAINT [PK_proposal_version_detail_criteria_programs] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_proposal_version_criteria_programs_proposal_version_criterias] FOREIGN KEY ([proposal_version_detail_id]) REFERENCES [dbo].[proposal_version_details] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_proposal_version_criteria_programs_proposal_version_criterias]
    ON [dbo].[proposal_version_detail_criteria_programs]([proposal_version_detail_id] ASC);

