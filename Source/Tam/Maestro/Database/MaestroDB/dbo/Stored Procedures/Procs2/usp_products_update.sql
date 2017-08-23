CREATE PROCEDURE usp_products_update
(
	@id		Int,
	@name		VarChar(127),
	@description		VarChar(255),
	@company_id		Int,
	@default_rate_card_type_id		Int,
	@geo_sensitive_comment		VarChar(2047),
	@pol_sensitive_comment		VarChar(2047),
	@date_created		DateTime,
	@date_last_modified		DateTime,
	@display_name		VarChar(63)
)
AS
UPDATE products SET
	name = @name,
	description = @description,
	company_id = @company_id,
	default_rate_card_type_id = @default_rate_card_type_id,
	geo_sensitive_comment = @geo_sensitive_comment,
	pol_sensitive_comment = @pol_sensitive_comment,
	date_created = @date_created,
	date_last_modified = @date_last_modified,
	display_name = @display_name
WHERE
	id = @id

