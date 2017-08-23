CREATE PROCEDURE usp_contacts_insert
(
	@id		Int		OUTPUT,
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
INSERT INTO contacts
(
	company_id,
	salutation_id,
	first_name,
	last_name,
	title,
	department,
	assistant,
	assistant_title,
	date_created,
	date_last_modified
)
VALUES
(
	@company_id,
	@salutation_id,
	@first_name,
	@last_name,
	@title,
	@department,
	@assistant,
	@assistant_title,
	@date_created,
	@date_last_modified
)

SELECT
	@id = SCOPE_IDENTITY()

