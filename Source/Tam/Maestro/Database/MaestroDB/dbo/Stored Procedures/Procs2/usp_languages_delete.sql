CREATE PROCEDURE [dbo].[usp_languages_delete]
	@id		TinyInt
AS
DELETE FROM
	dbo.languages
WHERE
	id = @id
