CREATE PROCEDURE usp_nielsen_network_rating_daypart_histories_insert
(
	@nielsen_network_id		Int,
	@daypart_id		Int,
	@start_date		DateTime,
	@end_date		DateTime
)
AS
INSERT INTO nielsen_network_rating_daypart_histories
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

