CREATE PROCEDURE usp_network_traffic_daypart_histories_insert
(
	@nielsen_network_id		Int,
	@daypart_id		Int,
	@start_date		DateTime,
	@end_date		DateTime
)
AS
INSERT INTO network_traffic_daypart_histories
(
	nielsen_network_id,
	daypart_id,
	start_date,
	end_date
)
VALUES
(
	@nielsen_network_id,
	@daypart_id,
	@start_date,
	@end_date
)

