CREATE PROCEDURE usp_emails_insert
(
	@id		Int		OUTPUT,
	@email_type_id		Int,
	@email		VarChar(127),
	@date_created		DateTime,
	@date_last_modified		DateTime
)
AS
INSERT INTO emails
(
	email_type_id,
	email,
	date_created,
	date_last_modified
)
VALUES
(
	@email_type_id,
	@email,
	@date_created,
	@date_last_modified
)

SELECT
	@id = SCOPE_IDENTITY()

