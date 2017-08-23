/****** Object:  View [dbo].[uvw_zonenetwork_timezone_offsets]    Script Date: 9/17/2014 2:12:24 PM ******/  
CREATE VIEW [dbo].[uvw_zonenetwork_timezone_offsets]  
AS  
	SELECT
		zn_tzi.zone_id,
		zn_tzi.network_id,
		zn_tzi.nielsen_network_id,
		f_tz.utc_offset - l_tz.utc_offset feed_offset
	FROM
		dbo.zone_network_time_zone_info zn_tzi (NOLOCK)
		JOIN dbo.time_zones f_tz (NOLOCK) ON f_tz.id = zn_tzi.feed_time_zone_id
		JOIN dbo.time_zones l_tz (NOLOCK) ON l_tz.id = zn_tzi.local_time_zone_id;