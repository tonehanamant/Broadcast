CREATE PROCEDURE usp_reels_delete
(
	@id Int
)
AS
DELETE FROM reels WHERE id=@id
