CREATE PROCEDURE usp_nielsen_network_rating_dayparts_select_all
AS
SELECT
	*
FROM
	nielsen_network_rating_dayparts WITH(NOLOCK)
