CREATE TABLE [dbo].[nti_transmittals_audiences] (
    [id]                                      INT        IDENTITY (1, 1) NOT NULL,
    [nti_transmittals_file_report_id]         INT        NOT NULL,
    [proposal_version_detail_quarter_week_id] INT        NOT NULL,
    [audience_id]                             INT        NOT NULL,
    [impressions]                             FLOAT (53) NOT NULL,
    CONSTRAINT [PK_nti_transmittals_audiences] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_nti_transmittals_audiences_audiences] FOREIGN KEY ([audience_id]) REFERENCES [dbo].[audiences] ([id]),
    CONSTRAINT [FK_nti_transmittals_audiences_nti_transmittals_file_reports] FOREIGN KEY ([nti_transmittals_file_report_id]) REFERENCES [dbo].[nti_transmittals_file_reports] ([id]) ON DELETE CASCADE,
    CONSTRAINT [FK_nti_transmittals_audiences_proposal_version_detail_quarter_weeks] FOREIGN KEY ([proposal_version_detail_quarter_week_id]) REFERENCES [dbo].[proposal_version_detail_quarter_weeks] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_nti_transmittals_audiences_proposal_version_detail_quarter_weeks]
    ON [dbo].[nti_transmittals_audiences]([proposal_version_detail_quarter_week_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_nti_transmittals_audiences_nti_transmittals_file_reports]
    ON [dbo].[nti_transmittals_audiences]([nti_transmittals_file_report_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_nti_transmittals_audiences_audiences]
    ON [dbo].[nti_transmittals_audiences]([audience_id] ASC);

