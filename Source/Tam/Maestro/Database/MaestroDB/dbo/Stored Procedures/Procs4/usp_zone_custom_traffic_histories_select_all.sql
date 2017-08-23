	
CREATE PROCEDURE usp_zone_custom_traffic_histories_select_all
AS
BEGIN
SELECT
	*
FROM
	dbo.zone_custom_traffic_histories WITH(NOLOCK)
END
