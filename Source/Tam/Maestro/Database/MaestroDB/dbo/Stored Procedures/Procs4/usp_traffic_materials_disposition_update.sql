CREATE PROCEDURE usp_traffic_materials_disposition_update
(
	@id		Int,
	@disposition		VarChar(127),
	@active		Bit,
	@sort_order		Int
)
AS
UPDATE traffic_materials_disposition SET
	disposition = @disposition,
	active = @active,
	sort_order = @sort_order
WHERE
	id = @id

