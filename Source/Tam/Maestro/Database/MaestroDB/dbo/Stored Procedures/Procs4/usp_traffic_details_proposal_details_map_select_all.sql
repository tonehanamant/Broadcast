CREATE PROCEDURE usp_traffic_details_proposal_details_map_select_all
AS
SELECT
	*
FROM
	traffic_details_proposal_details_map WITH(NOLOCK)
