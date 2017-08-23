CREATE PROCEDURE usp_expert_biases_delete
(
	@id Int
)
AS
DELETE FROM expert_biases WHERE id=@id
