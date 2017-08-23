-- =============================================
-- Author:		?
-- Modified By:	Stephen DeFusco
-- Create date: 8/25/2014
-- Description:	Rewrote to improve query performance.
-- =============================================
CREATE PROCEDURE [dbo].[usp_REL_GetDistinctOrdersForReleaseAndSystem]
	@release_id INT,
	@system_id INT
AS
BEGIN
	SELECT
		t.*
	FROM
		traffic t (NOLOCK)
	WHERE
		t.id IN (
			SELECT DISTINCT
				td.traffic_id
			FROM
				traffic_orders tro (NOLOCK) 
				join traffic_details td (NOLOCK) on td.id = tro.traffic_detail_id
				join traffic_detail_weeks tdw (NOLOCK) on tdw.traffic_detail_id = td.id 
					AND tro.start_date >= tdw.start_date AND tro.end_date <= tdw.end_date 
					AND tdw.suspended = 0 
			WHERE 
				tro.system_id = @system_id 
				AND tro.release_id = @release_id 
				AND tro.ordered_spots > 0 
				AND tro.active = 1
		)
	ORDER BY
		t.sort_order
		
	--select distinct 
	--	  traffic.*
	--from 
	--	  traffic_orders (NOLOCK) 
	--	  join traffic_details (NOLOCK) on traffic_details.id = traffic_orders.traffic_detail_id
	--	  join traffic_detail_weeks (NOLOCK) on traffic_detail_weeks.traffic_detail_id = traffic_details.id 
	--		and traffic_orders.start_date >= traffic_detail_weeks.start_date and traffic_orders.end_date <= traffic_orders.end_date 
	--	  join traffic (NOLOCK) on traffic.id = traffic_details.traffic_id
	--where 
	--	  traffic_orders.system_id = @system_id 
	--	  and traffic.release_id = @release_id and traffic_orders.ordered_spots > 0 
	--		and traffic_detail_weeks.suspended = 0 and traffic_orders.active = 1
	--order by
	--	  traffic.sort_order;
END
