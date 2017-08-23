CREATE PROCEDURE usp_campaigns_delete
(
	@id Int
)
AS
DELETE FROM campaigns WHERE id=@id
