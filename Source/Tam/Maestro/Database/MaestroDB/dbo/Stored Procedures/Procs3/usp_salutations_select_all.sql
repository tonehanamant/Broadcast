CREATE PROCEDURE usp_salutations_select_all
AS
SELECT
	*
FROM
	salutations WITH(NOLOCK)
