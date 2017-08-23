CREATE PROCEDURE usp_salutations_select
(
	@id Int
)
AS
SELECT
	*
FROM
	salutations WITH(NOLOCK)
WHERE
	id = @id
