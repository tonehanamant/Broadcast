
CREATE VIEW uvw_broadcast_daypart_timezones AS
SELECT     
	id, 
	broadcast_daypart_id, 
	timezone_id,
	daypart_id,
	effective_date AS start_date, 
	NULL AS end_date
FROM         
	dbo.broadcast_daypart_timezones WITH (NOLOCK)
UNION ALL
SELECT     
	id, 
	broadcast_daypart_id, 
	timezone_id,
	daypart_id,
	start_date, 
	end_date
FROM
    dbo.broadcast_daypart_timezone_histories WITH (NOLOCK)

