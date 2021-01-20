CREATE TABLE [dbo].[proposal_version_detail_quarter_weeks] (
    [id]                            INT             IDENTITY (1, 1) NOT NULL,
    [proposal_version_quarter_id]   INT             NOT NULL,
    [media_week_id]                 INT             NOT NULL,
    [start_date]                    DATETIME        NOT NULL,
    [end_date]                      DATETIME        NOT NULL,
    [is_hiatus]                     BIT             NOT NULL,
    [units]                         INT             NOT NULL,
    [impressions_goal]              FLOAT (53)      NOT NULL,
    [cost]                          DECIMAL (19, 4) NOT NULL,
    [open_market_impressions_total] FLOAT (53)      NOT NULL,
    [open_market_cost_total]        DECIMAL (19, 4) NOT NULL,
    [proprietary_impressions_total] FLOAT (53)      NOT NULL,
    [proprietary_cost_total]        DECIMAL (19, 4) NOT NULL,
    [myevents_report_name]          VARCHAR (25)    NULL,
    CONSTRAINT [PK_proposal_version_detail_quarter_weeks] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_proposal_version_detail_quarter_weeks_media_weeks] FOREIGN KEY ([media_week_id]) REFERENCES [dbo].[media_weeks] ([id]),
    CONSTRAINT [FK_proposal_version_detail_quarter_weeks_proposal_version_detail_quarters] FOREIGN KEY ([proposal_version_quarter_id]) REFERENCES [dbo].[proposal_version_detail_quarters] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_proposal_version_detail_quarter_weeks_proposal_version_detail_quarters]
    ON [dbo].[proposal_version_detail_quarter_weeks]([proposal_version_quarter_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_proposal_version_detail_quarter_weeks_media_weeks]
    ON [dbo].[proposal_version_detail_quarter_weeks]([media_week_id] ASC);

