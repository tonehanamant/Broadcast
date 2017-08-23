-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 4/8/2014
-- Description:	Test
-- =============================================
-- usp_PLS_GetGreedyAlgorithmTestData 528,46
CREATE PROCEDURE [dbo].[usp_PLS_GetGreedyAlgorithmTestData]
	@media_week_id INT,
	@network_id INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @month_start_date DATETIME;
	DECLARE @month_end_date DATETIME;
	DECLARE @week_start_date DATETIME;
	DECLARE @week_end_date DATETIME;
	DECLARE @media_month_id INT;

	SELECT
		@media_month_id = mm.id,
		@month_start_date = mm.start_date,
		@month_end_date = mm.end_date,
		@week_start_date = mw.start_date,
		@week_end_date = mw.end_date
	FROM
		media_months mm (NOLOCK)
		JOIN media_weeks mw (NOLOCK) ON mw.media_month_id=mm.id
	WHERE
		mw.id=@media_week_id;

	
	SELECT
		td.traffic_id,
		tbz.billing_system_id,
		tbz.billing_zone_id,
		tcbp.component_daypart_id,
		tcbp.traffic_daypart_id,
		tcbp.percentage,
		tro.ordered_spots,
		tro.ordered_spots * tcbp.percentage 'daypart_ordered_spots',
		CAST(CASE WHEN ISNULL(tro.subscribers, zn_t.subscribers) > 0 THEN tro.ordered_spot_rate * (CAST(zn_b.subscribers AS FLOAT) / CAST(ISNULL(tro.subscribers, zn_t.subscribers) AS FLOAT)) ELSE tro.ordered_spot_rate END AS MONEY) 'scaled_ordered_spot_rate',
		tro.ordered_spot_rate 'trafficked_ordered_spot_rate',
		zn_b.subscribers 'billing_zone_subscribers',
		td.spot_length_id,
		tcbp.intersecting_component_traffic_daypart_percentage
	FROM
		traffic_orders tro (NOLOCK)
		JOIN releases r (NOLOCK) ON r.id=tro.release_id
			AND r.status_id=9
		JOIN ##traffic_to_billing_zones tbz ON tbz.traffic_system_id=tro.system_id
			AND tbz.traffic_zone_id=tro.zone_id
			AND tro.start_date between @week_start_date and @week_end_date
		JOIN traffic_details td (NOLOCK) ON td.id=tro.traffic_detail_id
			AND td.network_id=CASE @network_id WHEN 60 THEN 35 WHEN 137 THEN 47 ELSE @network_id END
		JOIN traffic_detail_weeks tdw (NOLOCK) ON tdw.traffic_detail_id=td.id
			AND (tdw.start_date <= tro.end_date AND tdw.end_date >= tro.start_date)
			AND tdw.suspended=0
		JOIN media_weeks mw (NOLOCK) ON tro.start_date BETWEEN mw.start_date AND mw.end_date
			AND mw.id=@media_week_id
		JOIN ##trafficked_component_breakout_percentages tcbp ON tcbp.traffic_daypart_id=tro.daypart_id
		JOIN uvw_zonenetwork_universe zn_b ON zn_b.zone_id=tbz.billing_zone_id
			AND zn_b.network_id=tro.display_network_id
			AND zn_b.start_date<=@month_start_date AND (zn_b.end_date>=@month_start_date OR zn_b.end_date IS NULL)
		JOIN uvw_zonenetwork_universe zn_t ON zn_t.zone_id=tbz.traffic_zone_id
			AND zn_t.network_id=tro.display_network_id
			AND zn_t.start_date<=@month_start_date AND (zn_t.end_date>=@month_start_date OR zn_t.end_date IS NULL)
	WHERE
		tro.start_date BETWEEN @week_start_date AND @week_end_date
		AND tro.release_id IS NOT NULL
		AND tro.on_financial_reports=1
		AND tro.active=1
	ORDER BY
		tro.ordered_spot_rate DESC
END

