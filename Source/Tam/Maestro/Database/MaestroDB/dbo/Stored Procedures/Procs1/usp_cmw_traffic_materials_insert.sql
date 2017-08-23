CREATE PROCEDURE usp_cmw_traffic_materials_insert
(
	@cmw_traffic_id		Int,
	@cmw_material_id		Int,
	@rotation		Int,
	@disposition		VarChar(255),
	@sensitive		Bit,
	@comment		VarChar(255),
	@start_date		DateTime,
	@end_date		DateTime
)
AS
INSERT INTO cmw_traffic_materials
(
	cmw_traffic_id,
	cmw_material_id,
	rotation,
	disposition,
	sensitive,
	comment,
	start_date,
	end_date
)
VALUES
(
	@cmw_traffic_id,
	@cmw_material_id,
	@rotation,
	@disposition,
	@sensitive,
	@comment,
	@start_date,
	@end_date
)

