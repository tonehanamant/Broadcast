CREATE PROCEDURE usp_countries_select
(
	@id Int
)
AS
SELECT
	*
FROM
	countries WITH(NOLOCK)
WHERE
	id = @id
