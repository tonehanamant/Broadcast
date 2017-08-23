CREATE PROCEDURE [dbo].[usp_materials_insert]
(
	@id		int		OUTPUT,
	@product_id		Int,
	@spot_length_id		Int,
	@code		VarChar(31),
	@original_code		VarChar(31),
	@title		VarChar(255),
	@type		VarChar(15),
	@url		VarChar(1023),
	@phone_type		TinyInt,
	@phone_number		VarChar(15),
	@date_received		DateTime,
	@date_created		DateTime,
	@date_last_modified		DateTime,
	@tape_log		Bit,
	@tape_log_disposition		VarChar(1027),
	@active		Bit,
	@is_hd		Bit,
	@is_house_isci		Bit,
	@real_material_id		Int,
	@has_screener		Bit,
	@language_id        Tinyint
)
AS
INSERT INTO materials
(
	product_id,
	spot_length_id,
	code,
	original_code,
	title,
	type,
	url,
	phone_type,
	phone_number,
	date_received,
	date_created,
	date_last_modified,
	tape_log,
	tape_log_disposition,
	active,
	is_hd,
	is_house_isci,
	real_material_id,
	has_screener,
	language_id
)
VALUES
(
	@product_id,
	@spot_length_id,
	@code,
	@original_code,
	@title,
	@type,
	@url,
	@phone_type,
	@phone_number,
	@date_received,
	@date_created,
	@date_last_modified,
	@tape_log,
	@tape_log_disposition,
	@active,
	@is_hd,
	@is_house_isci,
	@real_material_id,
	@has_screener,
	@language_id
)
SELECT
	@id = SCOPE_IDENTITY()
