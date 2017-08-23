CREATE PROCEDURE usp_outlook_exports_select
(
	@id Int
)
AS
SELECT
	*
FROM
	outlook_exports WITH(NOLOCK)
WHERE
	id = @id
