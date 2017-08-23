CREATE TABLE [dbo].[daily_traffic_inventory_forecast_details] (
    [media_month_id]          SMALLINT   NOT NULL,
    [media_week_id]           INT        NOT NULL,
    [business_id]             INT        NOT NULL,
    [network_id]              INT        NOT NULL,
    [component_daypart_id]    INT        NOT NULL,
    [hh_eq_cpm_start]         MONEY      NOT NULL,
    [hh_eq_cpm_end]           MONEY      NOT NULL,
    [base_media_month_id]     SMALLINT   NOT NULL,
    [traffic_id]              INT        NOT NULL,
    [traffic_spot_target_id]  INT        NOT NULL,
    [needed_subscribers]      BIGINT     NOT NULL,
    [allocated_subscribers]   BIGINT     NOT NULL,
    [unallocated_subscribers] BIGINT     NOT NULL,
    [needed_units]            FLOAT (53) NOT NULL,
    [allocated_units]         FLOAT (53) NOT NULL,
    [unallocated_units]       FLOAT (53) NOT NULL,
    CONSTRAINT [PK_daily_traffic_inventory_forecast_details] PRIMARY KEY CLUSTERED ([media_month_id] ASC, [media_week_id] ASC, [business_id] ASC, [network_id] ASC, [component_daypart_id] ASC, [hh_eq_cpm_start] ASC, [hh_eq_cpm_end] ASC, [traffic_id] ASC, [traffic_spot_target_id] ASC) ON [MediaMonthSmallintScheme] ([media_month_id])
);

