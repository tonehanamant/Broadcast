CREATE PROCEDURE usp_system_statement_contact_infos_update
(
	@id		Int,
	@system_statement_group_id		Int,
	@system_id		Int,
	@first_name		VarChar(63),
	@last_name		VarChar(63),
	@email_address		VarChar(255)
)
AS
UPDATE system_statement_contact_infos SET
	system_statement_group_id = @system_statement_group_id,
	system_id = @system_id,
	first_name = @first_name,
	last_name = @last_name,
	email_address = @email_address
WHERE
	id = @id

