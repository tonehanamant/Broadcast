
CREATE PROCEDURE [dbo].[usp_REL_GetTrafficStartDateForSystem]
    @release_id INT,
    @system_id INT
AS
BEGIN
    SELECT
        MIN(tro.start_date)
    FROM
        traffic_orders tro (NOLOCK)
        JOIN traffic_details td (NOLOCK) on td.id = tro.traffic_detail_id
        JOIN traffic t (NOLOCK) ON t.id=td.traffic_id
        LEFT JOIN traffic_detail_weeks tdw (NOLOCK) on tdw.traffic_detail_id = td.id 
            AND tro.start_date >= tdw.start_date AND tro.end_date <= tdw.end_date 
        LEFT JOIN traffic_spot_targets tst (NOLOCK) ON tst.id=tro.traffic_spot_target_id    
    WHERE
        tro.system_id = @system_id 
        AND tro.release_id = @release_id
        AND ((t.plan_type=0 AND tdw.suspended = 0) OR (t.plan_type=1 AND tst.suspended=0))
	and tro.media_month_id in (select id from uvw_release_media_months where release_id = @release_id)
	and tro.traffic_id in (select id from traffic where release_id = @release_id )
END
