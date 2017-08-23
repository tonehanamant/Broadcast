CREATE PROCEDURE usp_rotational_biases_select
(
	@id Int
)
AS
SELECT
	*
FROM
	rotational_biases WITH(NOLOCK)
WHERE
	id = @id
