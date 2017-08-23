CREATE PROCEDURE usp_proposal_topographies_delete
(
	@topography_id		Int,
	@proposal_id		Int)
AS
DELETE FROM
	proposal_topographies
WHERE
	topography_id = @topography_id
 AND
	proposal_id = @proposal_id
