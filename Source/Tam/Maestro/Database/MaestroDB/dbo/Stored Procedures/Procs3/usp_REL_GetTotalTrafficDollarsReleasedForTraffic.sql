-- =============================================
-- Author:		Joe Jacobs
-- Create date: ?
-- Modified:	12/20/2013 - Stephen DeFusco - For NET to GROSS change added support to raise 
-- Description:	<Description,,>
-- =============================================
-- usp_REL_GetTotalTrafficDollarsReleasedForTraffic 35928
CREATE Procedure [dbo].[usp_REL_GetTotalTrafficDollarsReleasedForTraffic]
	@traffic_id INT
AS
BEGIN
	DECLARE @release_id INT
	DECLARE @start_date_of_release DATETIME
	
	-- get the release_id of the traffic order
	SELECT TOP 1
		@release_id = tro.release_id
	FROM
		traffic_details td (NOLOCK) 
		JOIN traffic_orders tro (NOLOCK) ON td.id = tro.traffic_detail_id
	WHERE 
		td.traffic_id = @traffic_id 
		AND tro.ordered_spots > 0 
		AND tro.on_financial_reports = 1
		AND tro.active = 1
		
	-- get the start date of all orders in the release
	SELECT
		@start_date_of_release = MIN(tro.start_date)
	FROM
		traffic_details td (NOLOCK) 
		JOIN traffic_orders tro (NOLOCK) ON td.id = tro.traffic_detail_id
	WHERE 
		td.traffic_id = @traffic_id 
		AND tro.ordered_spots > 0 
		AND tro.on_financial_reports = 1
		AND tro.active = 1
	
	-- get total GROSS cost of the release
	SELECT 
		-- 0x02000000 (if bit is turned on it means the system is NET, off is GROSS)
		-- if NET we divide the ordered_spot_rate by the systems spot_yield_rate to bring the ordered_spot_rate to GROSS
		SUM(tro.ordered_spots * CASE s.traffic_order_format & 0x02000000 WHEN 0x02000000 THEN tro.ordered_spot_rate / s.spot_yield_weight ELSE tro.ordered_spot_rate END) 
	FROM
		traffic_details td (NOLOCK) 
		JOIN traffic_orders tro (NOLOCK) ON td.id = tro.traffic_detail_id
		JOIN uvw_system_universe s ON s.system_id=tro.system_id
			AND (s.start_date<=@start_date_of_release AND (s.end_date>=@start_date_of_release OR s.end_date IS NULL))
	WHERE 
		td.traffic_id = @traffic_id 
		AND tro.ordered_spots > 0 
		AND tro.on_financial_reports = 1
		AND tro.active = 1
END
