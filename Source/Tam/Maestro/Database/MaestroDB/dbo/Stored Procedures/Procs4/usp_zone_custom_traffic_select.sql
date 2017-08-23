	
CREATE PROCEDURE usp_zone_custom_traffic_select
(
	@zone_id		Int
)
AS
BEGIN
SELECT
	*
FROM
	dbo.zone_custom_traffic WITH(NOLOCK)
WHERE
	zone_id=@zone_id

END
