CREATE PROCEDURE usp_topography_states_select_all
AS
SELECT
	*
FROM
	topography_states WITH(NOLOCK)
