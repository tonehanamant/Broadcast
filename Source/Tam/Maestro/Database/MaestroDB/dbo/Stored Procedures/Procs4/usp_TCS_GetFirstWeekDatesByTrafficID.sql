
CREATE PROCEDURE [dbo].[usp_TCS_GetFirstWeekDatesByTrafficID]
(
	@traffic_id int
)
AS
BEGIN
SELECT 
	MIN(traffic_detail_weeks.start_date), 
	MIN(traffic_detail_weeks.end_date) 
FROM 
	traffic_details (NOLOCK)
	join traffic_detail_weeks (NOLOCK)
		ON traffic_details.id = traffic_detail_weeks.traffic_detail_id 
WHERE 
	traffic_details.traffic_id = @traffic_id;

END

