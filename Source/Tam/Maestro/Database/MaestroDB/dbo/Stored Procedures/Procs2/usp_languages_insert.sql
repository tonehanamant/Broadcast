
CREATE PROCEDURE [dbo].[usp_languages_insert]
(
	@id		TinyInt,
	@code		VarChar(15),
	@name		VarChar(63)
)
AS
INSERT INTO dbo.languages
(
	id,
	code,
	name
)
VALUES
(
	@id,
	@code,
	@name
)
