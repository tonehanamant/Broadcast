CREATE PROCEDURE usp_expert_biases_select
(
	@id Int
)
AS
SELECT
	*
FROM
	expert_biases WITH(NOLOCK)
WHERE
	id = @id
