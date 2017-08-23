CREATE PROCEDURE usp_outlook_emails_insert
(
	@id		Int		OUTPUT,
	@email_type_id		Int,
	@email		VarChar(127)
)
AS
INSERT INTO outlook_emails
(
	email_type_id,
	email
)
VALUES
(
	@email_type_id,
	@email
)

SELECT
	@id = SCOPE_IDENTITY()

