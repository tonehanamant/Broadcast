CREATE PROCEDURE usp_system_statement_contact_infos_insert
(
	@id		Int		OUTPUT,
	@system_statement_group_id		Int,
	@system_id		Int,
	@first_name		VarChar(63),
	@last_name		VarChar(63),
	@email_address		VarChar(255)
)
AS
INSERT INTO system_statement_contact_infos
(
	system_statement_group_id,
	system_id,
	first_name,
	last_name,
	email_address
)
VALUES
(
	@system_statement_group_id,
	@system_id,
	@first_name,
	@last_name,
	@email_address
)

SELECT
	@id = SCOPE_IDENTITY()

