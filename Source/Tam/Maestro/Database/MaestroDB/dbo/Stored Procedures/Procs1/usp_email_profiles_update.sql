CREATE PROCEDURE usp_email_profiles_update
(
	@id		Int,
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
UPDATE email_profiles SET
	email_address = @email_address,
	host = @host,
	user_name = @user_name,
	password = @password,
	enabled = @enabled,
	display_name = @display_name,
	date_created = @date_created,
	date_last_modified = @date_last_modified
WHERE
	id = @id

