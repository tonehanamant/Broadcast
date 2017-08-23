CREATE PROCEDURE usp_proposal_materials_update
(
	@proposal_id		Int,
	@material_id		Int,
	@start_date		DateTime,
	@end_date		DateTime
)
AS
UPDATE proposal_materials SET
	start_date = @start_date,
	end_date = @end_date
WHERE
	proposal_id = @proposal_id AND
	material_id = @material_id
