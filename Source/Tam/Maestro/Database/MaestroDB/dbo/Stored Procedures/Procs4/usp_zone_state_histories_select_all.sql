CREATE PROCEDURE usp_zone_state_histories_select_all
AS
SELECT
	*
FROM
	zone_state_histories WITH(NOLOCK)
