CREATE PROCEDURE [dbo].[usp_mit_universes_delete]
(
	@id Int
)
AS
DELETE FROM dbo.mit_universes WHERE id=@id
