CREATE PROCEDURE usp_topographies_delete
(
	@id Int
)
AS
DELETE FROM topographies WHERE id=@id
