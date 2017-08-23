CREATE PROCEDURE usp_salutations_delete
(
	@id Int
)
AS
DELETE FROM salutations WHERE id=@id
