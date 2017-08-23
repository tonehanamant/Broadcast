CREATE PROCEDURE usp_dayparts_delete
(
	@id Int
)
AS
DELETE FROM dayparts WHERE id=@id
