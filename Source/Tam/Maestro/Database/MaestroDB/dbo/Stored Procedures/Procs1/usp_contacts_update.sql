CREATE PROCEDURE usp_contacts_update
(
	@id		Int,
	@company_id		Int,
	@salutation_id		Int,
	@first_name		VarChar(63),
	@last_name		VarChar(63),
	@title		VarChar(63),
	@department		VarChar(63),
	@assistant		VarChar(63),
	@assistant_title		VarChar(63),
	@date_created		DateTime,
	@date_last_modified		DateTime
)
AS
UPDATE contacts SET
	company_id = @company_id,
	salutation_id = @salutation_id,
	first_name = @first_name,
	last_name = @last_name,
	title = @title,
	department = @department,
	assistant = @assistant,
	assistant_title = @assistant_title,
	date_created = @date_created,
	date_last_modified = @date_last_modified
WHERE
	id = @id

