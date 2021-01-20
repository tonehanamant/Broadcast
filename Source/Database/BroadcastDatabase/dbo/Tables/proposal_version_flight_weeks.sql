CREATE TABLE [dbo].[proposal_version_flight_weeks] (
    [id]                  INT      IDENTITY (1, 1) NOT NULL,
    [proposal_version_id] INT      NOT NULL,
    [media_week_id]       INT      NOT NULL,
    [start_date]          DATETIME NOT NULL,
    [end_date]            DATETIME NOT NULL,
    [active]              BIT      NOT NULL,
    CONSTRAINT [PK_proposal_version_flight_weeks] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_proposal_version_flight_weeks_media_weeks] FOREIGN KEY ([media_week_id]) REFERENCES [dbo].[media_weeks] ([id]),
    CONSTRAINT [FK_proposal_version_flight_weeks_proposal_versions] FOREIGN KEY ([proposal_version_id]) REFERENCES [dbo].[proposal_versions] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_proposal_version_flight_weeks_proposal_versions]
    ON [dbo].[proposal_version_flight_weeks]([proposal_version_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_proposal_version_flight_weeks_media_weeks]
    ON [dbo].[proposal_version_flight_weeks]([media_week_id] ASC);

