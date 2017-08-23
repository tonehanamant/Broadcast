CREATE PROCEDURE usp_outlook_contacts_insert
(
	@id		Int		OUTPUT,
	@outlook_export_id		Int,
	@outlook_company_id		Int,
	@salutation_id		Int,
	@first_name		VarChar(63),
	@last_name		VarChar(63),
	@title		VarChar(63),
	@department		VarChar(63),
	@assistant		VarChar(63),
	@assistant_title		VarChar(63),
	@web_page_address		VarChar(512),
	@im_address		VarChar(512)
)
AS
INSERT INTO outlook_contacts
(
	outlook_export_id,
	outlook_company_id,
	salutation_id,
	first_name,
	last_name,
	title,
	department,
	assistant,
	assistant_title,
	web_page_address,
	im_address
)
VALUES
(
	@outlook_export_id,
	@outlook_company_id,
	@salutation_id,
	@first_name,
	@last_name,
	@title,
	@department,
	@assistant,
	@assistant_title,
	@web_page_address,
	@im_address
)

SELECT
	@id = SCOPE_IDENTITY()

