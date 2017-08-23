CREATE PROCEDURE usp_traffic_audiences_select_all
AS
SELECT
	*
FROM
	traffic_audiences WITH(NOLOCK)
