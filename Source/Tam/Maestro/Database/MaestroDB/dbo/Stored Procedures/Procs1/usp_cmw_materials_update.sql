CREATE PROCEDURE usp_cmw_materials_update
(
	@id		Int,
	@spot_length_id		Int,
	@cmw_traffic_product_id		Int,
	@code		VarChar(31),
	@title		VarChar(255),
	@date_created		DateTime,
	@date_last_modified		DateTime
)
AS
UPDATE cmw_materials SET
	spot_length_id = @spot_length_id,
	cmw_traffic_product_id = @cmw_traffic_product_id,
	code = @code,
	title = @title,
	date_created = @date_created,
	date_last_modified = @date_last_modified
WHERE
	id = @id

