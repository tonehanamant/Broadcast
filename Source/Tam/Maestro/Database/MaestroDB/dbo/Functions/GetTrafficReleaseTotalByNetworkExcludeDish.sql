-- =============================================
-- Author:        Stephen DeFusco
-- Create date: 7/22/2009
-- Description:   
-- =============================================
--select * from GetTrafficReleaseTotalByNetworkExcludeDish(2479)

CREATE FUNCTION [dbo].[GetTrafficReleaseTotalByNetworkExcludeDish]
(     
      @traffic_id INT
)

RETURNS @traffic_release_totals TABLE
(
      traffic_id INT,
      network_id Int,
      daypart_id Int,
      traffic_detail_id int,
      ordered_spots int,
      ordered_spot_rate money,
      total_dollars money,
      subscribers int
)
AS
BEGIN
      INSERT INTO @traffic_release_totals
      
      SELECT 
                  traffic_details.traffic_id,
                  traffic_details.network_id,
                  traffic_orders.daypart_id,
                  traffic_orders.traffic_detail_id,
                  SUM(traffic_orders.ordered_spots),
                  SUM(traffic_orders.ordered_spot_rate),
                  SUM(traffic_orders.ordered_spots * traffic_orders.ordered_spot_rate),
                  dbo.GetActiveSubsInZoneByTrafficDetailIdWithoutDish(traffic_orders.traffic_detail_id)
            from
                  traffic_orders (NOLOCK)
                  join traffic_details (NOLOCK) on traffic_orders.traffic_detail_id = traffic_details.id
                  join traffic_detail_weeks tdw WITH (NOLOCK) on tdw.traffic_detail_id = traffic_details.id 
                  and traffic_orders.start_date >= tdw.start_date and traffic_orders.end_date <= tdw.end_date 
            where
                  traffic_details.traffic_id = @traffic_id
                  and traffic_orders.on_financial_reports = 1
                  and tdw.suspended = 0
                  and traffic_orders.active = 1
                  and traffic_orders.system_id <> 67 and traffic_orders.system_id <> 668
            group by
                  traffic_details.traffic_id,
                  traffic_details.network_id,
                  traffic_orders.daypart_id,
                  traffic_orders.traffic_detail_id
      RETURN;
END
