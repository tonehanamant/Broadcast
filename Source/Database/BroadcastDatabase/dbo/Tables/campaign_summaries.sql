CREATE TABLE [dbo].[campaign_summaries] (
    [id]                                INT             IDENTITY (1, 1) NOT NULL,
    [campaign_id]                       INT             NOT NULL,
    [processing_status]                 INT             NOT NULL,
    [processing_status_error_msg]       NVARCHAR (2000) NULL,
    [queued_at]                         DATETIME        NOT NULL,
    [queued_by]                         VARCHAR (50)    NOT NULL,
    [flight_start_Date]                 DATETIME        NULL,
    [flight_end_Date]                   DATETIME        NULL,
    [flight_hiatus_days]                INT             NULL,
    [flight_active_days]                INT             NULL,
    [budget]                            FLOAT (53)      NULL,
    [plan_status_count_working]         INT             NULL,
    [plan_status_count_reserved]        INT             NULL,
    [plan_status_count_client_approval] INT             NULL,
    [plan_status_count_contracted]      INT             NULL,
    [plan_status_count_live]            INT             NULL,
    [plan_status_count_complete]        INT             NULL,
    [campaign_status]                   INT             NULL,
    [components_modified]               DATETIME        NULL,
    [last_aggregated]                   DATETIME        NULL,
    [plan_status_count_scenario]        INT             NULL,
    [plan_status_count_canceled]        INT             NULL,
    [plan_status_count_rejected]        INT             NULL,
    [hh_cpm]                            FLOAT (53)      NULL,
    [hh_impressions]                    FLOAT (53)      NULL,
    [hh_rating_points]                  FLOAT (53)      NULL,
    CONSTRAINT [PK_campaign_summaries] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_campaign_summaries_campaign] FOREIGN KEY ([campaign_id]) REFERENCES [dbo].[campaigns] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_campaign_summaries_campaign]
    ON [dbo].[campaign_summaries]([campaign_id] ASC);

