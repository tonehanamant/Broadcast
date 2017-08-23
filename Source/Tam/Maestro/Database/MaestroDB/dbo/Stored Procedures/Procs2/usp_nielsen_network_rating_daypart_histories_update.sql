CREATE PROCEDURE usp_nielsen_network_rating_daypart_histories_update
(
	@nielsen_network_id		Int,
	@daypart_id		Int,
	@start_date		DateTime,
	@end_date		DateTime
)
AS
UPDATE nielsen_network_rating_daypart_histories SET
	end_date = @end_date
WHERE
	nielsen_network_id = @nielsen_network_id AND
	daypart_id = @daypart_id AND
	start_date = @start_date
