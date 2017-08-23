	
CREATE PROCEDURE usp_zone_custom_traffic_select_all
AS
BEGIN
SELECT
	*
FROM
	dbo.zone_custom_traffic WITH(NOLOCK)
END
