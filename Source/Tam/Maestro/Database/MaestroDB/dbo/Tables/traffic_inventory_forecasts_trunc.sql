CREATE TABLE [dbo].[traffic_inventory_forecasts_trunc] (
    [base_media_month_id]     SMALLINT NOT NULL,
    [forecast_media_month_id] SMALLINT NOT NULL,
    [forecast_media_week_id]  INT      NOT NULL,
    [business_id]             INT      NOT NULL,
    [network_id]              INT      NOT NULL,
    [component_daypart_id]    INT      NOT NULL,
    [hh_eq_cpm_start]         MONEY    NOT NULL,
    [hh_eq_cpm_end]           MONEY    NOT NULL,
    [subscribers]             BIGINT   NOT NULL,
    CONSTRAINT [PK_traffic_inventory_forecasts_trunc] PRIMARY KEY CLUSTERED ([base_media_month_id] ASC, [forecast_media_month_id] ASC, [forecast_media_week_id] ASC, [business_id] ASC, [network_id] ASC, [component_daypart_id] ASC, [hh_eq_cpm_start] ASC, [hh_eq_cpm_end] ASC) ON [MediaMonthSmallintScheme] ([base_media_month_id])
);

