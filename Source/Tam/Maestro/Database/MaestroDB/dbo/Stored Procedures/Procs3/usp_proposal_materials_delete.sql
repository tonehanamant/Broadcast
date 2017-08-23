CREATE PROCEDURE usp_proposal_materials_delete
(
	@proposal_id		Int,
	@material_id		Int)
AS
DELETE FROM
	proposal_materials
WHERE
	proposal_id = @proposal_id
 AND
	material_id = @material_id
