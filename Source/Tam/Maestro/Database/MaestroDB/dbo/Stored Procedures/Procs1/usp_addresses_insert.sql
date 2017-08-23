CREATE PROCEDURE usp_addresses_insert
(
	@id		Int		OUTPUT,
	@name		VarChar(63),
	@address_type_id		Int,
	@address_line_1		VarChar(127),
	@address_line_2		VarChar(127),
	@address_line_3		VarChar(127),
	@city		VarChar(63),
	@state_id		Int,
	@zip		VarChar(15),
	@country_id		Int,
	@date_created		DateTime,
	@date_last_modified		DateTime
)
AS
INSERT INTO addresses
(
	name,
	address_type_id,
	address_line_1,
	address_line_2,
	address_line_3,
	city,
	state_id,
	zip,
	country_id,
	date_created,
	date_last_modified
)
VALUES
(
	@name,
	@address_type_id,
	@address_line_1,
	@address_line_2,
	@address_line_3,
	@city,
	@state_id,
	@zip,
	@country_id,
	@date_created,
	@date_last_modified
)

SELECT
	@id = SCOPE_IDENTITY()

