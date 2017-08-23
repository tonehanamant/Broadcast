CREATE PROCEDURE usp_network_break_histories_select
(
	@network_break_id		Int,
	@start_date		DateTime
)
AS
SELECT
	*
FROM
	network_break_histories WITH(NOLOCK)
WHERE
	network_break_id=@network_break_id
	AND
	start_date=@start_date

