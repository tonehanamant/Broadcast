
CREATE PROCEDURE [dbo].[usp_rating_sources_delete]
(
	@id		TinyInt)
AS
DELETE FROM
	rating_sources
WHERE
	id = @id

