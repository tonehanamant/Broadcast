CREATE PROCEDURE usp_nielsen_suggested_rates_select
(
	@id Int
)
AS
SELECT
	*
FROM
	nielsen_suggested_rates WITH(NOLOCK)
WHERE
	id = @id
