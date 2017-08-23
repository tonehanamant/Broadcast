CREATE PROCEDURE usp_nielsen_network_histories_select
(
	@nielsen_network_id		Int,
	@start_date		DateTime
)
AS
SELECT
	*
FROM
	nielsen_network_histories WITH(NOLOCK)
WHERE
	nielsen_network_id=@nielsen_network_id
	AND
	start_date=@start_date

