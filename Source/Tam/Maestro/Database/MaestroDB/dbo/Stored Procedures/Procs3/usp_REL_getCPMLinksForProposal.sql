


CREATE PROCEDURE [dbo].[usp_REL_getCPMLinksForProposal]
(
	@proposal_id Int,
	@traffic_id int
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
	proposal_id = @proposal_id and traffic_id = @traffic_id order by id


