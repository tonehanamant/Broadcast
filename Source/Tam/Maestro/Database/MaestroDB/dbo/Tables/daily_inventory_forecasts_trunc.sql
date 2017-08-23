CREATE TABLE [dbo].[daily_inventory_forecasts_trunc] (
    [media_month_id]                SMALLINT NOT NULL,
    [media_week_id]                 INT      NOT NULL,
    [network_id]                    INT      NOT NULL,
    [component_daypart_id]          INT      NOT NULL,
    [hh_eq_cpm_start]               MONEY    NOT NULL,
    [hh_eq_cpm_end]                 MONEY    NOT NULL,
    [base_media_month_id]           SMALLINT NOT NULL,
    [total_forecasted_subscribers]  BIGINT   NOT NULL,
    [total_needed_subscribers]      BIGINT   NOT NULL,
    [total_allocated_subscribers]   BIGINT   NOT NULL,
    [total_unallocated_subscribers] BIGINT   NOT NULL,
    CONSTRAINT [PK_daily_inventory_forecasts_trunc] PRIMARY KEY CLUSTERED ([media_month_id] ASC, [media_week_id] ASC, [network_id] ASC, [component_daypart_id] ASC, [hh_eq_cpm_start] ASC, [hh_eq_cpm_end] ASC) ON [MediaMonthSmallintScheme] ([media_month_id])
);

