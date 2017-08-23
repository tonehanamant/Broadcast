	CREATE PROCEDURE usp_zone_universe_start_date_get
	(
		@zone_id int
	)
	AS
	BEGIN
		SELECT MIN([start_date]) AS [start_date] 
		FROM uvw_zone_universe 
		WHERE zone_id = @zone_id
	END
