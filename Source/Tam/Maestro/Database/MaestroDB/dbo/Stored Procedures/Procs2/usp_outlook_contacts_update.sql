CREATE PROCEDURE usp_outlook_contacts_update
(
	@id		Int,
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
UPDATE outlook_contacts SET
	outlook_export_id = @outlook_export_id,
	outlook_company_id = @outlook_company_id,
	salutation_id = @salutation_id,
	first_name = @first_name,
	last_name = @last_name,
	title = @title,
	department = @department,
	assistant = @assistant,
	assistant_title = @assistant_title,
	web_page_address = @web_page_address,
	im_address = @im_address
WHERE
	id = @id

