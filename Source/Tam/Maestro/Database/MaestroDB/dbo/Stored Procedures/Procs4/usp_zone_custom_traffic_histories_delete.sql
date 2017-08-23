	
CREATE PROCEDURE usp_zone_custom_traffic_histories_delete
(
	@zone_id		Int,
	@start_date		DateTime)
AS
BEGIN
DELETE FROM
	dbo.zone_custom_traffic_histories
WHERE
	zone_id = @zone_id
 AND
	start_date = @start_date
END
