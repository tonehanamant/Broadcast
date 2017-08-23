

-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 7/22/2009
-- Description:	
-- =============================================
--select * from GetTrafficReleaseTotalByNetworkExcludeDish(2479)

CREATE FUNCTION [dbo].[GetActiveSubsInZoneByTrafficDetailIdWithoutDish]
(	
	@traffic_detail_id INT
)

RETURNS int

AS
BEGIN
declare @answer int;

	SELECT
		@answer =  
		sum(traffic_orders.subscribers) / count(distinct traffic_orders.start_date)
	from
		traffic_orders (NOLOCK)
		join traffic_details (NOLOCK) on traffic_orders.traffic_detail_id = traffic_details.id
	where
		traffic_details.id = @traffic_detail_id
		and traffic_orders.on_financial_reports = 1
		and traffic_orders.system_id <> 67 and traffic_orders.system_id <> 668
	RETURN @answer;
END

