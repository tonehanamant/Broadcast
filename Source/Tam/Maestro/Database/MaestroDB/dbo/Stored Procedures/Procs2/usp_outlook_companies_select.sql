CREATE PROCEDURE usp_outlook_companies_select
(
	@id Int
)
AS
SELECT
	*
FROM
	outlook_companies WITH(NOLOCK)
WHERE
	id = @id
