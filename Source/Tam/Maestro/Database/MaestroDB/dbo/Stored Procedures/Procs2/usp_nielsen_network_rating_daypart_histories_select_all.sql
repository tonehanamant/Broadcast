CREATE PROCEDURE usp_nielsen_network_rating_daypart_histories_select_all
AS
SELECT
	*
FROM
	nielsen_network_rating_daypart_histories WITH(NOLOCK)
