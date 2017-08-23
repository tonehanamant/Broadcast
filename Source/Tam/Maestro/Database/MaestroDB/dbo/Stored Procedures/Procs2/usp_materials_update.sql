CREATE PROCEDURE [dbo].[usp_materials_update]
(
	@id		Int,
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
	@language_id		TinyInt
)
AS
UPDATE dbo.materials SET
	product_id = @product_id,
	spot_length_id = @spot_length_id,
	code = @code,
	original_code = @original_code,
	title = @title,
	type = @type,
	url = @url,
	phone_type = @phone_type,
	phone_number = @phone_number,
	date_received = @date_received,
	date_created = @date_created,
	date_last_modified = @date_last_modified,
	tape_log = @tape_log,
	tape_log_disposition = @tape_log_disposition,
	active = @active,
	is_hd = @is_hd,
	is_house_isci = @is_house_isci,
	real_material_id = @real_material_id,
	has_screener = @has_screener,
	language_id = @language_id
WHERE
	id = @id
