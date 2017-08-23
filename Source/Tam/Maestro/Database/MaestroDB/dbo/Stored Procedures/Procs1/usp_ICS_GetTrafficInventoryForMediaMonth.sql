
CREATE PROCEDURE [dbo].[usp_ICS_GetTrafficInventoryForMediaMonth]
      @media_month_id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		traffic_details.traffic_id,
		traffic_details.network_id,
		traffic.status_id,
		SUM(traffic_detail_topographies.spots * spot_lengths.delivery_multiplier) 'units_per_month',
		vw_ccc_daypart.id 'daypart_id',
		vw_ccc_daypart.code,
		vw_ccc_daypart.name,
		vw_ccc_daypart.start_time,
		vw_ccc_daypart.end_time,
		vw_ccc_daypart.mon,
		vw_ccc_daypart.tue,
		vw_ccc_daypart.wed,
		vw_ccc_daypart.thu,
		vw_ccc_daypart.fri,
		vw_ccc_daypart.sat,
		vw_ccc_daypart.sun,
		traffic_detail_topographies.topography_id
	FROM 
		traffic_details (NOLOCK)
		JOIN traffic (NOLOCK) ON traffic.id=traffic_details.traffic_id
		JOIN traffic_detail_weeks (NOLOCK) on traffic_detail_weeks.traffic_detail_id = traffic_details.id
		JOIN traffic_detail_topographies (NOLOCK) ON traffic_detail_topographies.traffic_detail_week_id = traffic_detail_weeks.id
		JOIN spot_lengths (NOLOCK) ON spot_lengths.id=traffic_details.spot_length_id
		JOIN vw_ccc_daypart (NOLOCK) ON vw_ccc_daypart.id=traffic_details.daypart_id 
		JOIN media_months mm (NOLOCK) ON
			((traffic_detail_weeks.start_date between mm.start_date and mm.end_date) 
			or
			(traffic_detail_weeks.end_date between mm.start_date and mm.end_date))
	WHERE 
		mm.id = @media_month_id 
		-- 5=In Progress, 12=Ready for Order, 14=Indexed
		AND (traffic.status_id = 5 OR traffic.status_id = 12 OR traffic.status_id = 14)
		AND 
		(
			traffic_detail_weeks.suspended = 0 
		)
	GROUP BY
		traffic_details.traffic_id,
		traffic_details.network_id,
		traffic.status_id,
		vw_ccc_daypart.id,
		vw_ccc_daypart.code,
		vw_ccc_daypart.name,
		vw_ccc_daypart.start_time,
		vw_ccc_daypart.end_time,
		vw_ccc_daypart.mon,
		vw_ccc_daypart.tue,
		vw_ccc_daypart.wed,
		vw_ccc_daypart.thu,
		vw_ccc_daypart.fri,
		vw_ccc_daypart.sat,
		vw_ccc_daypart.sun,
		traffic_detail_topographies.topography_id

END
