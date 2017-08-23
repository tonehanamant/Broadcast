/***************************************************************************************************
** Date         Author          Description   
** ---------    ----------		-----------------------------------------------------
** XXXXX		Stephen DeFusco
** 04/07/2017	Abdul Sukkur	Added @flight_start_date
*****************************************************************************************************/
CREATE PROCEDURE [dbo].[usp_REL_GetZoneFactorTrafficDetailsForSystemInsertionByWeek]
	@traffic_id INT,
	@system_id INT,
	@flight_start_date datetime = NULL
AS
BEGIN
	SELECT 
		tro.traffic_detail_id, 
		n.code 'network', 
		sl.[length],
		tro.daypart_id, 
		tro.start_date, 
		tro.end_date,
		SUM(tro.ordered_spot_rate) 'rate', 
		SUM(tro.ordered_spot_rate * tro.ordered_spots) 'total_revenue', 
		SUM(tro.subscribers) 'subscribers',
		ts.start_time,
		ts.end_time
	FROM
		dbo.traffic_orders tro				(NOLOCK)
		JOIN dbo.traffic_details td			(NOLOCK) ON td.id = tro.traffic_detail_id
		JOIN dbo.traffic_detail_weeks tdw	(NOLOCK) ON tdw.traffic_detail_id = td.id 
		AND tro.start_date >= tdw.start_date AND tro.end_date <= tdw.end_date 
		JOIN dbo.traffic t					(NOLOCK) ON t.id = td.traffic_id
		JOIN dbo.uvw_network_universe n		(NOLOCK) ON td.network_id = n.network_id 
		AND (n.start_date<=t.start_date AND (n.end_date>=t.start_date OR n.end_date IS NULL))
		JOIN dbo.spot_lengths sl			(NOLOCK) ON sl.id = td.spot_length_id
		join dayparts dp (NOLOCK)  on dp.id = tro.daypart_id 
		join timespans ts (NOLOCK)  on ts.id = dp.timespan_id
	WHERE 
		tro.traffic_id = @traffic_id
		AND tro.system_id = @system_id 
		AND t.id = @traffic_id
		AND tro.ordered_spots > 0
		AND tdw.suspended = 0
		AND tro.active = 1
		AND (@flight_start_date is null or tro.end_date >= @flight_start_date) 
	GROUP BY 
		tro.traffic_detail_id, 
		n.code, 
		tro.daypart_id, 
		tro.start_date, 
		tro.end_date,
		sl.[length],
		ts.start_time,
		ts.end_time
	ORDER BY 
	  n.code, 
  	  tro.start_date
END
