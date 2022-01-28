CREATE TABLE [dbo].[plan_version_weekly_breakdown] (
    [id]                     INT             IDENTITY (1, 1) NOT NULL,
    [media_week_id]          INT             NOT NULL,
    [number_active_days]     INT             NOT NULL,
    [active_days_label]      VARCHAR (20)    NULL,
    [impressions]            FLOAT (53)      NOT NULL,
    [impressions_percentage] FLOAT (53)      NOT NULL,
    [plan_version_id]        INT             NOT NULL,
    [rating_points]          FLOAT (53)      NOT NULL,
    [budget]                 DECIMAL (19, 4) NOT NULL,
    [spot_length_id]         INT             NULL,
    [percentage_of_week]     FLOAT (53)      NULL,
    [adu_impressions]        FLOAT (53)      NOT NULL,
    [unit_impressions]       FLOAT (53)      NULL,
    [standard_daypart_id]    INT             NULL,
    [is_locked] BIT NULL, 
    [custom_daypart_organization_id] INT NULL, 
    [custom_daypart_name] NVARCHAR(100) NULL, 
    CONSTRAINT [PK_plan_version_weekly_breakdown] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_plan_version_weekly_breakdown_media_weeks] FOREIGN KEY ([media_week_id]) REFERENCES [dbo].[media_weeks] ([id]),
    CONSTRAINT [FK_plan_version_weekly_breakdown_plan_versions] FOREIGN KEY ([plan_version_id]) REFERENCES [dbo].[plan_versions] ([id]) ON DELETE CASCADE,
    CONSTRAINT [FK_plan_version_weekly_breakdown_spot_lengths] FOREIGN KEY ([spot_length_id]) REFERENCES [dbo].[spot_lengths] ([id]),
    CONSTRAINT [FK_plan_version_weekly_breakdown_standard_dayparts] FOREIGN KEY ([standard_daypart_id]) REFERENCES [dbo].[standard_dayparts] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_version_weekly_breakdown_standard_dayparts]
    ON [dbo].[plan_version_weekly_breakdown]([standard_daypart_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_version_weekly_breakdown_spot_lengths]
    ON [dbo].[plan_version_weekly_breakdown]([spot_length_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_version_weekly_breakdown_plan_versions]
    ON [dbo].[plan_version_weekly_breakdown]([plan_version_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_version_weekly_breakdown_media_weeks]
    ON [dbo].[plan_version_weekly_breakdown]([media_week_id] ASC);

