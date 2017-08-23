
CREATE PROCEDURE [dbo].[usp_rating_sources_update]
(
	@id		TinyInt,
	@code		VarChar(31),
	@name		VarChar(255)
)
AS
UPDATE rating_sources SET
	code = @code,
	name = @name
WHERE
	id = @id

