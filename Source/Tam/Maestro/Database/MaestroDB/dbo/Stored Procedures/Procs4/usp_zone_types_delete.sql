CREATE PROCEDURE usp_zone_types_delete
(
	@id Int
)
AS
BEGIN
DELETE FROM zone_types WHERE id=@id
END
