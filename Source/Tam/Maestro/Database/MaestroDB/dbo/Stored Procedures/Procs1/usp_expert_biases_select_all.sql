CREATE PROCEDURE usp_expert_biases_select_all
AS
SELECT
	*
FROM
	expert_biases WITH(NOLOCK)
