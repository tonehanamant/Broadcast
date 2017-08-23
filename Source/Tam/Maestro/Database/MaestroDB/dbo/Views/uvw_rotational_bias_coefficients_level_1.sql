CREATE VIEW [dbo].[uvw_rotational_bias_coefficients_level_1]
WITH SCHEMABINDING
AS
    SELECT 
		rbc.rule_code,
		rbc.base_media_month_id,
		rbc.forecast_media_month_id,
		rbc.week_number,
		rbc.network_id,
		rbc.hour_of_week,
		SUM(CAST(rbc.subscribers AS BIGINT)) 'subscribers',
		COUNT_BIG(*) 'num_businesses'
    FROM 
		dbo.rotational_bias_coefficients rbc
    GROUP BY 
		rbc.rule_code,
		rbc.base_media_month_id,
		rbc.forecast_media_month_id,
		rbc.week_number,
		rbc.network_id,
		rbc.hour_of_week;
GO
CREATE UNIQUE CLUSTERED INDEX [IX_uvw_rotational_bias_coefficients_level_1]
    ON [dbo].[uvw_rotational_bias_coefficients_level_1]([rule_code] ASC, [base_media_month_id] ASC, [forecast_media_month_id] ASC, [week_number] ASC, [network_id] ASC, [hour_of_week] ASC);

