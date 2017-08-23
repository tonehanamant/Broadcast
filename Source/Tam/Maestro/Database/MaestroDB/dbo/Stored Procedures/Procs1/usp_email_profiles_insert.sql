CREATE PROCEDURE usp_email_profiles_insert
(
	@id		Int		OUTPUT,
	@email_address		VarChar(63),
	@host		VarChar(127),
	@user_name		VarChar(63),
	@password		VarChar(255),
	@enabled		Bit,
	@display_name		VarChar(127),
	@date_created		DateTime,
	@date_last_modified		DateTime
)
AS
INSERT INTO email_profiles
(
	email_address,
	host,
	user_name,
	password,
	enabled,
	display_name,
	date_created,
	date_last_modified
)
VALUES
(
	@email_address,
	@host,
	@user_name,
	@password,
	@enabled,
	@display_name,
	@date_created,
	@date_last_modified
)

SELECT
	@id = SCOPE_IDENTITY()

