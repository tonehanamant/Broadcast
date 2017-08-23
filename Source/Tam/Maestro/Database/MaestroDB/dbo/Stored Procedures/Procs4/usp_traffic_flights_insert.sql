CREATE PROCEDURE usp_traffic_flights_insert
(
	@traffic_id		Int,
	@start_date		DateTime,
	@end_date		DateTime,
	@selected		Bit
)
AS
INSERT INTO traffic_flights
(
	traffic_id,
	start_date,
	end_date,
	selected
)
VALUES
(
	@traffic_id,
	@start_date,
	@end_date,
	@selected
)

