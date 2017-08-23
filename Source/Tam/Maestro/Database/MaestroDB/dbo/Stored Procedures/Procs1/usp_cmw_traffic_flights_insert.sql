CREATE PROCEDURE usp_cmw_traffic_flights_insert
(
	@cmw_traffic_id		Int,
	@start_date		DateTime,
	@end_date		DateTime,
	@selected		Bit
)
AS
INSERT INTO cmw_traffic_flights
(
	cmw_traffic_id,
	start_date,
	end_date,
	selected
)
VALUES
(
	@cmw_traffic_id,
	@start_date,
	@end_date,
	@selected
)

