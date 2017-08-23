-- usp_PCS_GetTrafficDeliveryForPlan 38773,31
CREATE PROCEDURE [dbo].[usp_PCS_GetTrafficDeliveryForPlan]
(
	@traffic_id INT,
	@audience_id INT
)
AS
BEGIN
	SELECT 
		start_date,
		delivery
	FROM 
		dbo.GetTotalDeliveryForTrafficByWeekAndAudience (@traffic_id, @audience_id)
	ORDER BY
		start_date
END


