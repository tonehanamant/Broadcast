CREATE TABLE [dbo].[proposal_version_detail_criteria_cpm] (
    [id]                         INT             IDENTITY (1, 1) NOT NULL,
    [proposal_version_detail_id] INT             NOT NULL,
    [min_max]                    TINYINT         NOT NULL,
    [value]                      DECIMAL (19, 4) NOT NULL,
    CONSTRAINT [PK_proposal_version_detail_criteria_cpm] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_proposal_version_detail_criteria_cpm_proposal_version_details] FOREIGN KEY ([proposal_version_detail_id]) REFERENCES [dbo].[proposal_version_details] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_proposal_version_detail_criteria_cpm_proposal_version_details]
    ON [dbo].[proposal_version_detail_criteria_cpm]([proposal_version_detail_id] ASC);

