CREATE PROCEDURE usp_cmw_traffic_materials_update
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
UPDATE cmw_traffic_materials SET
	rotation = @rotation,
	disposition = @disposition,
	sensitive = @sensitive,
	comment = @comment,
	start_date = @start_date,
	end_date = @end_date
WHERE
	cmw_traffic_id = @cmw_traffic_id AND
	cmw_material_id = @cmw_material_id
