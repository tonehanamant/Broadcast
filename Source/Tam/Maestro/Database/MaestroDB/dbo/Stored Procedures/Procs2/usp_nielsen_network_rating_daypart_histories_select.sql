CREATE PROCEDURE usp_nielsen_network_rating_daypart_histories_select
(
	@nielsen_network_id		Int,
	@daypart_id		Int,
	@start_date		DateTime
)
AS
SELECT
	*
FROM
	nielsen_network_rating_daypart_histories WITH(NOLOCK)
WHERE
	nielsen_network_id=@nielsen_network_id
	AND
	daypart_id=@daypart_id
	AND
	start_date=@start_date

