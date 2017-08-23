

-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 7/22/2009
-- Description:	
-- =============================================
Create FUNCTION [dbo].[GetTrafficReleaseTotalBySystemAndNetwork]
(	
	@traffic_id INT,
	@system_id int
)

RETURNS @traffic_release_totals TABLE
(
	traffic_id INT,
	system_id INT,
	network_id Int,
	daypart_id Int,
	total_dollars money,
	on_financial_reports bit
)
AS
BEGIN
	INSERT INTO @traffic_release_totals
		SELECT 
			traffic_details.traffic_id,
			traffic_orders.system_id,
			traffic_details.network_id,
			traffic_orders.daypart_id,
			SUM(traffic_orders.ordered_spots * traffic_orders.ordered_spot_rate),
			traffic_orders.on_financial_reports
		from
			traffic_orders (NOLOCK)
		join 
			traffic_details (NOLOCK) on traffic_orders.traffic_detail_id = traffic_details.id
		where
			traffic_details.traffic_id = @traffic_id
			and traffic_orders.system_id = @system_id
		group by
			traffic_details.traffic_id,
			traffic_orders.system_id,
			traffic_details.network_id,
			traffic_orders.daypart_id,
			traffic_orders.on_financial_reports
	RETURN;
END
