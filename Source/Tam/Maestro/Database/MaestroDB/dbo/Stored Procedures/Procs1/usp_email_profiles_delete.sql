CREATE PROCEDURE usp_email_profiles_delete
(
	@id Int
)
AS
DELETE FROM email_profiles WHERE id=@id
