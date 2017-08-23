
CREATE PROCEDURE [dbo].[usp_REL_GetTrafficStartDateForAllSystems]
    @release_id INT
AS
BEGIN
    -- This is to avoid parameter sniffing, was causing a 100ms query to take 15 seconds.
    declare @local_release_id INT = @release_id
    declare @traffic_ids table(id int)

    SELECT
	tro.system_id,
        MIN(tro.start_date)
    FROM
        traffic_orders tro (NOLOCK)
        JOIN traffic_details td (NOLOCK) on td.id = tro.traffic_detail_id
        JOIN traffic t (NOLOCK) ON t.id=td.traffic_id
        LEFT JOIN traffic_detail_weeks tdw (NOLOCK) on tdw.traffic_detail_id = td.id 
            AND tro.start_date >= tdw.start_date AND tro.end_date <= tdw.end_date 
        LEFT JOIN traffic_spot_targets tst (NOLOCK) ON tst.id=tro.traffic_spot_target_id    
    WHERE
        tro.release_id = @local_release_id
        AND ((t.plan_type=0 AND tdw.suspended = 0) OR (t.plan_type=1 AND tst.suspended=0))
	and tro.media_month_id in (select id from uvw_release_media_months where release_id = @local_release_id)
	and tro.traffic_id in (select id from traffic where release_id = @local_release_id )
	GROUP BY tro.system_id
END