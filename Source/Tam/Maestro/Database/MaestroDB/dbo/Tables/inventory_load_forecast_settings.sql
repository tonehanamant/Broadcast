CREATE TABLE [dbo].[inventory_load_forecast_settings] (
    [base_media_month_id] SMALLINT   NOT NULL,
    [horizon]             INT        NOT NULL,
    [week_part]           BIT        NOT NULL,
    [network_id]          INT        NOT NULL,
    [alpha]               FLOAT (53) NOT NULL,
    CONSTRAINT [PK_inventory_load_forecast_settings] PRIMARY KEY CLUSTERED ([base_media_month_id] ASC, [horizon] ASC, [week_part] ASC, [network_id] ASC)
);

