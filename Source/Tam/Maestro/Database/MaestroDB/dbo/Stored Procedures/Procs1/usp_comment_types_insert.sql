
CREATE PROCEDURE [dbo].[usp_comment_types_insert]
(
	@id		Int		OUTPUT,
	@name		VarChar(50)
)
AS
INSERT INTO comment_types
(
	name
)
VALUES
(
	@name
)

SELECT
	@id = SCOPE_IDENTITY()


