CREATE TABLE [dbo].[plan_version_daypart_weekly_breakdown] (
    [id]                           INT          IDENTITY (1, 1) NOT NULL,
    [plan_version_daypart_goal_id] INT          NOT NULL,
    [media_week_id]                INT          NOT NULL,
    [number_active_days]           INT          NOT NULL,
    [active_days_label]            VARCHAR (20) NULL,
    [impressions]                  FLOAT (53)   NOT NULL,
    [impressions_percentage]       FLOAT (53)   NOT NULL,
    [plan_version_id]              INT          NOT NULL,
    [rating_points]                FLOAT (53)   NOT NULL,
    [budget]                       MONEY        NOT NULL,
    [spot_length_id]               INT          NULL,
    [percentage_of_week]           FLOAT (53)   NULL,
    [adu_impressions]              FLOAT (53)   NOT NULL,
    [unit_impressions]             FLOAT (53)   NULL,
    [is_locked]                    BIT          NULL,
    CONSTRAINT [FK_plan_version_daypart_weekly_breakdown_media_weeks] FOREIGN KEY ([media_week_id]) REFERENCES [dbo].[media_weeks] ([id]),
    CONSTRAINT [FK_plan_version_daypart_weekly_breakdown_plan_version_daypart_goals] FOREIGN KEY ([plan_version_daypart_goal_id]) REFERENCES [dbo].[plan_version_daypart_goals] ([id]) ON DELETE CASCADE,
    CONSTRAINT [FK_plan_version_daypart_weekly_breakdown_spot_lengths] FOREIGN KEY ([spot_length_id]) REFERENCES [dbo].[spot_lengths] ([id])
);

