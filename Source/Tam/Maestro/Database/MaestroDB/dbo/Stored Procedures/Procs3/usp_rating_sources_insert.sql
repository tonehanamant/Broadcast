
CREATE PROCEDURE [dbo].[usp_rating_sources_insert]
(
	@id		TinyInt,
	@code		VarChar(31),
	@name		VarChar(255)
)
AS
INSERT INTO rating_sources
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


