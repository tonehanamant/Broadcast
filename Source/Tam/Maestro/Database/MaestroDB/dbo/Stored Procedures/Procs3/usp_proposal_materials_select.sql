CREATE PROCEDURE usp_proposal_materials_select
(
	@proposal_id		Int,
	@material_id		Int
)
AS
SELECT
	*
FROM
	proposal_materials WITH(NOLOCK)
WHERE
	proposal_id=@proposal_id
	AND
	material_id=@material_id

