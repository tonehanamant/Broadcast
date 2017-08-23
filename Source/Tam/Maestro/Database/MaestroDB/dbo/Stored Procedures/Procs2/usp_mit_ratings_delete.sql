CREATE PROCEDURE [dbo].[usp_mit_ratings_delete]
(
	@id Int
)
AS
DELETE FROM dbo.mit_ratings WHERE id=@id
