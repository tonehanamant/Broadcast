CREATE PROCEDURE usp_network_types_delete
(
	@id TinyInt
)
AS
BEGIN
DELETE FROM dbo.network_types WHERE id=@id
END
