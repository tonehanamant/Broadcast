CREATE PROCEDURE usp_traffic_details_proposal_details_map_select
(
	@id Int
)
AS
SELECT
	*
FROM
	traffic_details_proposal_details_map WITH(NOLOCK)
WHERE
	id = @id
