CREATE PROCEDURE usp_system_statement_contact_infos_delete
(
	@id Int
)
AS
DELETE FROM system_statement_contact_infos WHERE id=@id
