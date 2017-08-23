CREATE PROCEDURE usp_rotational_biases_select_all
AS
SELECT
	*
FROM
	rotational_biases WITH(NOLOCK)
