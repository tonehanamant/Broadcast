CREATE TABLE [dbo].[tam_post_analysis_reports_dayparts] (
    [tam_post_proposal_id] INT        NOT NULL,
    [audience_id]          INT        NOT NULL,
    [media_week_id]        INT        NOT NULL,
    [network_id]           INT        NOT NULL,
    [daypart_id]           INT        NOT NULL,
    [enabled]              BIT        NOT NULL,
    [subscribers]          BIGINT     NOT NULL,
    [delivery]             FLOAT (53) NOT NULL,
    [eq_delivery]          FLOAT (53) NOT NULL,
    [units]                FLOAT (53) NOT NULL,
    [dr_delivery]          FLOAT (53) NOT NULL,
    [dr_eq_delivery]       FLOAT (53) NOT NULL,
    [total_spots]          INT        NOT NULL,
    CONSTRAINT [PK_tam_post_analysis_dayparts] PRIMARY KEY CLUSTERED ([tam_post_proposal_id] ASC, [audience_id] ASC, [media_week_id] ASC, [network_id] ASC, [daypart_id] ASC, [enabled] ASC),
    CONSTRAINT [FK_tam_post_analysis_reports_daypart_audiences] FOREIGN KEY ([audience_id]) REFERENCES [dbo].[audiences] ([id]),
    CONSTRAINT [FK_tam_post_analysis_reports_daypart_networks] FOREIGN KEY ([network_id]) REFERENCES [dbo].[networks] ([id]),
    CONSTRAINT [FK_tam_post_analysis_reports_daypart_tam_post_proposals] FOREIGN KEY ([tam_post_proposal_id]) REFERENCES [dbo].[tam_post_proposals] ([id]),
    CONSTRAINT [FK_tam_post_analysis_reports_dayparts_dayparts] FOREIGN KEY ([daypart_id]) REFERENCES [dbo].[dayparts] ([id]),
    CONSTRAINT [FK_tam_post_analysis_reports_dayparts_media_weeks] FOREIGN KEY ([media_week_id]) REFERENCES [dbo].[media_weeks] ([id])
);

