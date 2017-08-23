CREATE PROCEDURE usp_outlook_addresses_insert
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
	@country_id		Int
)
AS
INSERT INTO outlook_addresses
(
	name,
	address_type_id,
	address_line_1,
	address_line_2,
	address_line_3,
	city,
	state_id,
	zip,
	country_id
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
	@country_id
)

SELECT
	@id = SCOPE_IDENTITY()

