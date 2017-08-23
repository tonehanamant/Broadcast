CREATE PROCEDURE usp_rotational_biases_delete
(
	@id Int
)
AS
DELETE FROM rotational_biases WHERE id=@id
