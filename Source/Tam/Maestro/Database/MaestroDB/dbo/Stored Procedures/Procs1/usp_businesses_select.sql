CREATE PROCEDURE usp_businesses_select
(
	@id Int
)
AS
SELECT
	*
FROM
	businesses WITH(NOLOCK)
WHERE
	id = @id
