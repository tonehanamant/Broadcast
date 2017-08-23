CREATE PROCEDURE usp_spot_lengths_delete
(
	@id Int
)
AS
DELETE FROM spot_lengths WHERE id=@id
