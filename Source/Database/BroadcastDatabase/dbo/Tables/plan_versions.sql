CREATE TABLE [dbo].[plan_versions] (
    [id]                    INT             IDENTITY (1, 1) NOT NULL,
    [plan_id]               INT             NOT NULL,
    [is_draft]              BIT             NOT NULL,
    [equivalized]           BIT             NOT NULL,
    [flight_start_date]     DATETIME        NULL,
    [flight_end_date]       DATETIME        NULL,
    [flight_notes]          NVARCHAR (1024) NULL,
    [audience_type]         INT             NOT NULL,
    [posting_type]          INT             NOT NULL,
    [target_audience_id]    INT             NOT NULL,
    [share_book_id]         INT             NOT NULL,
    [hut_book_id]           INT             NULL,
    [budget]                MONEY           NULL,
    [target_impression]     FLOAT (53)      NULL,
    [target_cpm]            MONEY           NULL,
    [target_rating_points]  FLOAT (53)      NULL,
    [target_cpp]            MONEY           NULL,
    [target_universe]       FLOAT (53)      NULL,
    [hh_impressions]        FLOAT (53)      NULL,
    [hh_cpm]                MONEY           NULL,
    [hh_rating_points]      FLOAT (53)      NULL,
    [hh_cpp]                MONEY           NULL,
    [hh_universe]           FLOAT (53)      NULL,
    [currency]              INT             NULL,
    [target_vpvh]           FLOAT (53)      NOT NULL,
    [coverage_goal_percent] FLOAT (53)      NULL,
    [goal_breakdown_type]   INT             NULL,
    [status]                INT             NOT NULL,
    [created_by]            VARCHAR (63)    NOT NULL,
    [created_date]          DATETIME        NOT NULL,
    [modified_by]           VARCHAR (63)    NULL,
    [modified_date]         DATETIME        NULL,
    [version_number]        INT             NULL,
    [is_adu_enabled]        BIT             NULL,
    [impressions_per_unit]  FLOAT (53)      NULL,
    [flight_notes_internal] NVARCHAR (1024) NULL,
    CONSTRAINT [PK_plan_versions] PRIMARY KEY CLUSTERED ([id] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_plan_versions_audiences] FOREIGN KEY ([target_audience_id]) REFERENCES [dbo].[audiences] ([id]),
    CONSTRAINT [FK_plan_versions_hut_media_months] FOREIGN KEY ([hut_book_id]) REFERENCES [dbo].[media_months] ([id]),
    CONSTRAINT [FK_plan_versions_plans] FOREIGN KEY ([plan_id]) REFERENCES [dbo].[plans] ([id]),
    CONSTRAINT [FK_plan_versions_share_media_months] FOREIGN KEY ([share_book_id]) REFERENCES [dbo].[media_months] ([id])
);




GO



GO



GO



GO
CREATE NONCLUSTERED INDEX [IX_plan_versions_plan_id]
    ON [dbo].[plan_versions]([plan_id] ASC);

