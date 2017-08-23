CREATE TABLE [dbo].[rotational_bias_coefficients] (
    [rule_code]               TINYINT  NOT NULL,
    [base_media_month_id]     SMALLINT NOT NULL,
    [forecast_media_month_id] INT      NOT NULL,
    [week_number]             TINYINT  NOT NULL,
    [network_id]              INT      NOT NULL,
    [hour_of_week]            TINYINT  NOT NULL,
    [business_id]             INT      NOT NULL,
    [subscribers]             BIGINT   NOT NULL,
    CONSTRAINT [PK_rotational_bias_coefficients_1] PRIMARY KEY CLUSTERED ([rule_code] ASC, [base_media_month_id] ASC, [forecast_media_month_id] ASC, [week_number] ASC, [network_id] ASC, [hour_of_week] ASC, [business_id] ASC) WITH (FILLFACTOR = 90)
);

