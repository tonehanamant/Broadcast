
CREATE PROCEDURE [dbo].[usp_ACS_GetTimeZoneOffsets] 
AS
BEGIN
	SET NOCOUNT ON;
	
    SELECT
	zn.zone_id,
	zn.network_id,
	z.time_zone_id,
	zn.feed_type,
	f_tz.utc_offset - l_tz.utc_offset feed_offset
FROM
	uvw_zonenetwork_universe zn
	JOIN uvw_zone_universe z ON z.zone_id=zn.zone_id
	JOIN dbo.time_zones l_tz (NOLOCK) ON l_tz.id = z.time_zone_id
	JOIN dbo.time_zones f_tz (NOLOCK) ON f_tz.id = CASE zn.feed_type WHEN 1 THEN 1 WHEN 2 THEN 3 WHEN 3 THEN 4 END
WHERE
	time_zone_id IS NOT NULL
END
