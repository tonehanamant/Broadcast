CREATE PROCEDURE usp_cmw_contact_infos_delete
(
	@id Int
)
AS
DELETE FROM cmw_contact_infos WHERE id=@id
