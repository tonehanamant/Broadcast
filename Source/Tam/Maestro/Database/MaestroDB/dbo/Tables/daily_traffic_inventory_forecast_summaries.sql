CREATE TABLE [dbo].[daily_traffic_inventory_forecast_summaries] (
    [media_month_id]          INT        NOT NULL,
    [total_updates]           INT        NOT NULL,
    [number_proposals]        INT        NOT NULL,
    [number_proposal_details] INT        NOT NULL,
    [last_duration]           FLOAT (53) NOT NULL,
    [average_duration]        FLOAT (53) NOT NULL,
    [date_last_started]       DATETIME   NOT NULL,
    [date_last_completed]     DATETIME   NULL,
    CONSTRAINT [PK_daily_traffic_inventory_forecast_summaries] PRIMARY KEY CLUSTERED ([media_month_id] ASC)
);

