CREATE PROCEDURE usp_topography_state_histories_select_all
AS
SELECT
	*
FROM
	topography_state_histories WITH(NOLOCK)
