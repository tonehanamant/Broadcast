CREATE PROCEDURE usp_outlook_addresses_update
(
	@id		Int,
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
UPDATE outlook_addresses SET
	name = @name,
	address_type_id = @address_type_id,
	address_line_1 = @address_line_1,
	address_line_2 = @address_line_2,
	address_line_3 = @address_line_3,
	city = @city,
	state_id = @state_id,
	zip = @zip,
	country_id = @country_id
WHERE
	id = @id

