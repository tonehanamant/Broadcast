


-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
-- SELECT n.code, dbo.GetTrafficDetailCPMEquivalized(td.id,31) FROM traffic_details td JOIN networks n ON n.id=td.network_id WHERE traffic_id=2028 ORDER BY n.code
CREATE FUNCTION [dbo].[GetTrafficReleaseDishTotal]
(
	@traffic_id INT
)
RETURNS MONEY
AS
BEGIN
	DECLARE @return AS MONEY
	
	SET @return = (
		SELECT 
			SUM(traffic_orders.ordered_spots * traffic_orders.ordered_spot_rate)
		from
			traffic_orders (NOLOCK)
		join 
			traffic_details (NOLOCK) on traffic_orders.traffic_detail_id = traffic_details.id
		where
			traffic_details.traffic_id = @traffic_id
			and traffic_orders.system_id = 67 
	)

	RETURN @return
END
