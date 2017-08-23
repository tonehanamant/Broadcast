CREATE PROCEDURE usp_nielsen_suggested_rate_details_select
(
	@id Int
)
AS
SELECT
	*
FROM
	nielsen_suggested_rate_details WITH(NOLOCK)
WHERE
	id = @id
