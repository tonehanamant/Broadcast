CREATE TABLE [dbo].[plan_versions] (
    [id]                    INT             IDENTITY (1, 1) NOT NULL,
    [plan_id]               INT             NOT NULL,
    [is_draft]              BIT             NOT NULL,
    [equivalized]           BIT             NOT NULL,
    [flight_start_date]     DATETIME        NOT NULL,
    [flight_end_date]       DATETIME        NOT NULL,
    [flight_notes]          NVARCHAR (1024) NULL,
    [flight_notes_internal] NVARCHAR (1024) NULL,
    [audience_type]         INT             NOT NULL,
    [posting_type]          INT             NOT NULL,
    [target_audience_id]    INT             NOT NULL,
    [share_book_id]         INT             NOT NULL,
    [hut_book_id]           INT             NULL,
    [budget]                DECIMAL (19, 4) NOT NULL,
    [target_impression]     FLOAT (53)      NOT NULL,
    [target_cpm]            DECIMAL (19, 4) NOT NULL,
    [target_rating_points]  FLOAT (53)      NOT NULL,
    [target_cpp]            DECIMAL (19, 4) NOT NULL,
    [target_universe]       FLOAT (53)      NOT NULL,
    [hh_impressions]        FLOAT (53)      NOT NULL,
    [hh_cpm]                DECIMAL (19, 4) NOT NULL,
    [hh_rating_points]      FLOAT (53)      NOT NULL,
    [hh_cpp]                DECIMAL (19, 4) NOT NULL,
    [hh_universe]           FLOAT (53)      NOT NULL,
    [currency]              INT             NOT NULL,
    [target_vpvh]           FLOAT (53)      NOT NULL,
    [coverage_goal_percent] FLOAT (53)      NOT NULL,
    [goal_breakdown_type]   INT             NOT NULL,
    [status]                INT             NOT NULL,
    [created_by]            VARCHAR (63)    NOT NULL,
    [created_date]          DATETIME        NOT NULL,
    [modified_by]           VARCHAR (63)    NULL,
    [modified_date]         DATETIME        NULL,
    [version_number]        INT             NULL,
    [is_adu_enabled]        BIT             NOT NULL,
    [impressions_per_unit]  FLOAT (53)      NULL,
    CONSTRAINT [PK_plan_versions] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_plan_versions_audiences] FOREIGN KEY ([target_audience_id]) REFERENCES [dbo].[audiences] ([id]),
    CONSTRAINT [FK_plan_versions_hut_media_months] FOREIGN KEY ([hut_book_id]) REFERENCES [dbo].[media_months] ([id]),
    CONSTRAINT [FK_plan_versions_plans] FOREIGN KEY ([plan_id]) REFERENCES [dbo].[plans] ([id]),
    CONSTRAINT [FK_plan_versions_share_media_months] FOREIGN KEY ([share_book_id]) REFERENCES [dbo].[media_months] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_versions_plans]
    ON [dbo].[plan_versions]([plan_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_versions_share_media_months]
    ON [dbo].[plan_versions]([share_book_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_versions_hut_media_months]
    ON [dbo].[plan_versions]([hut_book_id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FK_plan_versions_audiences]
    ON [dbo].[plan_versions]([target_audience_id] ASC);

