CREATE PROCEDURE usp_proposal_topographies_insert
(
	@topography_id		Int,
	@proposal_id		Int
)
AS
INSERT INTO proposal_topographies
(
	topography_id,
	proposal_id
)
VALUES
(
	@topography_id,
	@proposal_id
)

