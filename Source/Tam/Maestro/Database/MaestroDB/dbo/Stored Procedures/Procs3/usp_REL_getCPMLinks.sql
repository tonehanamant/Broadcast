



CREATE PROCEDURE [dbo].[usp_REL_getCPMLinks]
(
	@traffic_id Int
)
AS
SELECT
	id,
	traffic_id,
	proposal_id,
	weighting_factor
FROM
	release_cpmlink (NOLOCK)
WHERE
	traffic_id = @traffic_id order by id



