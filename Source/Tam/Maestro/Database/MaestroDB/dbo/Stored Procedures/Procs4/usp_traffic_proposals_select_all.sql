CREATE PROCEDURE usp_traffic_proposals_select_all
AS
SELECT
	*
FROM
	traffic_proposals WITH(NOLOCK)
