CREATE PROCEDURE usp_states_select_all
AS
SELECT
	*
FROM
	states WITH(NOLOCK)
