CREATE PROCEDURE [dbo].[usp_BS_GetBroadcastDaypartMap]
	@map_set VARCHAR(63),
	@effective_date datetime
AS
BEGIN
	SELECT * FROM time_zones (NOLOCK)
	SELECT 
		d.id,
		d.code,
		d.name,
		d.start_time,
		d.end_time,
		d.mon,
		d.tue,
		d.wed,
		d.thu,
		d.fri,
		d.sat,
		d.sun 
	FROM 
		vw_ccc_daypart d WHERE d.id IN (
			SELECT DISTINCT daypart_id FROM uvw_broadcast_daypart_timezones (NOLOCK) 
			where 
			(uvw_broadcast_daypart_timezones.start_date<=@effective_date AND (uvw_broadcast_daypart_timezones.end_date>=@effective_date OR uvw_broadcast_daypart_timezones.end_date IS NULL))

		)
	SELECT * FROM broadcast_dayparts (NOLOCK) WHERE map_set='BDPRPSL_DYPRTS'
	SELECT * FROM uvw_broadcast_daypart_timezones (NOLOCK) 
	WHERE
		(uvw_broadcast_daypart_timezones.start_date<=@effective_date AND (uvw_broadcast_daypart_timezones.end_date>=@effective_date OR uvw_broadcast_daypart_timezones.end_date IS NULL))
	ORDER BY broadcast_daypart_id,timezone_id,daypart_id
END
