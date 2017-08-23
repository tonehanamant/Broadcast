CREATE PROCEDURE usp_proposal_materials_insert
(
	@proposal_id		Int,
	@material_id		Int,
	@start_date		DateTime,
	@end_date		DateTime
)
AS
INSERT INTO proposal_materials
(
	proposal_id,
	material_id,
	start_date,
	end_date
)
VALUES
(
	@proposal_id,
	@material_id,
	@start_date,
	@end_date
)

