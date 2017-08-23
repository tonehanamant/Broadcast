CREATE PROCEDURE usp_cmw_materials_insert
(
	@id		Int		OUTPUT,
	@spot_length_id		Int,
	@cmw_traffic_product_id		Int,
	@code		VarChar(31),
	@title		VarChar(255),
	@date_created		DateTime,
	@date_last_modified		DateTime
)
AS
INSERT INTO cmw_materials
(
	spot_length_id,
	cmw_traffic_product_id,
	code,
	title,
	date_created,
	date_last_modified
)
VALUES
(
	@spot_length_id,
	@cmw_traffic_product_id,
	@code,
	@title,
	@date_created,
	@date_last_modified
)

SELECT
	@id = SCOPE_IDENTITY()

