CREATE PROCEDURE usp_proposal_topographies_select
(
	@topography_id		Int,
	@proposal_id		Int
)
AS
SELECT
	*
FROM
	proposal_topographies WITH(NOLOCK)
WHERE
	topography_id=@topography_id
	AND
	proposal_id=@proposal_id

