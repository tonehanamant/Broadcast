CREATE PROCEDURE usp_zone_states_select_all
AS
SELECT
	*
FROM
	zone_states WITH(NOLOCK)
