CREATE PROCEDURE usp_products_insert
(
	@id		Int		OUTPUT,
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
INSERT INTO products
(
	name,
	description,
	company_id,
	default_rate_card_type_id,
	geo_sensitive_comment,
	pol_sensitive_comment,
	date_created,
	date_last_modified,
	display_name
)
VALUES
(
	@name,
	@description,
	@company_id,
	@default_rate_card_type_id,
	@geo_sensitive_comment,
	@pol_sensitive_comment,
	@date_created,
	@date_last_modified,
	@display_name
)

SELECT
	@id = SCOPE_IDENTITY()

