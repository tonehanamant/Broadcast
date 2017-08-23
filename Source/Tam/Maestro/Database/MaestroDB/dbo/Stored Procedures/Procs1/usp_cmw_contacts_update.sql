CREATE PROCEDURE usp_cmw_contacts_update
(
	@id		Int,
	@first_name		VarChar(63),
	@last_name		VarChar(63),
	@cmw_traffic_company_id		Int,
	@salutation_id		Int,
	@title		VarChar(63),
	@department		VarChar(63),
	@assistant		VarChar(63),
	@assistant_title		VarChar(63),
	@date_created		DateTime,
	@date_last_modified		DateTime,
	@address_line_1		VarChar(127),
	@address_line_2		VarChar(127),
	@address_line_3		VarChar(127),
	@city		VarChar(63),
	@state_id		Int,
	@zip		VarChar(15),
	@country_id		Int
)
AS
UPDATE cmw_contacts SET
	first_name = @first_name,
	last_name = @last_name,
	cmw_traffic_company_id = @cmw_traffic_company_id,
	salutation_id = @salutation_id,
	title = @title,
	department = @department,
	assistant = @assistant,
	assistant_title = @assistant_title,
	date_created = @date_created,
	date_last_modified = @date_last_modified,
	address_line_1 = @address_line_1,
	address_line_2 = @address_line_2,
	address_line_3 = @address_line_3,
	city = @city,
	state_id = @state_id,
	zip = @zip,
	country_id = @country_id
WHERE
	id = @id

