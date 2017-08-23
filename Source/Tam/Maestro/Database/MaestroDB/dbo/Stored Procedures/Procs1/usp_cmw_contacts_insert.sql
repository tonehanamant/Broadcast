CREATE PROCEDURE usp_cmw_contacts_insert
(
	@id		Int		OUTPUT,
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
INSERT INTO cmw_contacts
(
	first_name,
	last_name,
	cmw_traffic_company_id,
	salutation_id,
	title,
	department,
	assistant,
	assistant_title,
	date_created,
	date_last_modified,
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
	@first_name,
	@last_name,
	@cmw_traffic_company_id,
	@salutation_id,
	@title,
	@department,
	@assistant,
	@assistant_title,
	@date_created,
	@date_last_modified,
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

