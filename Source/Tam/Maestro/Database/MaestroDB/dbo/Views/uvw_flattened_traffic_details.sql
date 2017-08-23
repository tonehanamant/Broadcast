-- SELECT * FROM uvw_flattened_traffic_details
CREATE VIEW [dbo].[uvw_flattened_traffic_details]
AS
	SELECT
		tm.material_id,
		td.*,
		d.start_time,
		d.end_time,
		d.mon,
		d.tue,
		d.wed,
		d.thu,
		d.fri,
		d.sat,
		d.sun,
		d.daypart_text,
		tf.start_date [flight_start_date],
		tf.end_date [flight_end_date]
	FROM
		traffic_details				td	(NOLOCK)
		JOIN traffic				t	(NOLOCK) ON t.id=td.traffic_id
		JOIN vw_ccc_daypart			d	(NOLOCK) ON d.id=td.daypart_id
		JOIN traffic_flights		tf	(NOLOCK) ON tf.traffic_id=td.traffic_id AND tf.selected=1
		JOIN traffic_materials		tm	(NOLOCK) ON tm.traffic_id=td.traffic_id
	WHERE
		t.release_id IS NOT NULL