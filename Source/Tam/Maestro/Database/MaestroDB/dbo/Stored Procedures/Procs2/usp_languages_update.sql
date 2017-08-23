CREATE PROCEDURE [dbo].[usp_languages_update]
(
	@id		TinyInt,
	@code		VarChar(15),
	@name		VarChar(63)
)
AS
UPDATE dbo.languages SET
	code = @code,
	name = @name
WHERE
	id = @id
