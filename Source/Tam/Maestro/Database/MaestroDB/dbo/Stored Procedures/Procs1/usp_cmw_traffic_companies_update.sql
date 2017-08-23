CREATE PROCEDURE usp_cmw_traffic_companies_update
(
	@id		Int,
	@name		VarChar(63),
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
UPDATE cmw_traffic_companies SET
	name = @name,
	address_line_1 = @address_line_1,
	address_line_2 = @address_line_2,
	address_line_3 = @address_line_3,
	city = @city,
	state_id = @state_id,
	zip = @zip,
	country_id = @country_id,
	date_created = @date_created,
	date_last_modified = @date_last_modified
WHERE
	id = @id

