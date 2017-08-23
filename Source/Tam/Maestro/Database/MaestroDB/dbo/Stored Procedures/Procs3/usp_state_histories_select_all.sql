CREATE PROCEDURE usp_state_histories_select_all
AS
SELECT
	*
FROM
	state_histories WITH(NOLOCK)
