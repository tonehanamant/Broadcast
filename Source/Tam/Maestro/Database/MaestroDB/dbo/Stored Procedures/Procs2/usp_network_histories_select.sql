CREATE PROCEDURE usp_network_histories_select
(
	@network_id		Int,
	@start_date		DateTime
)
AS
BEGIN
SELECT
	*
FROM
	dbo.network_histories WITH(NOLOCK)
WHERE
	network_id=@network_id
	AND
	start_date=@start_date

END
