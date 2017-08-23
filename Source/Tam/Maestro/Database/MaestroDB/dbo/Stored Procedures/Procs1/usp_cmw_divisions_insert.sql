
CREATE PROCEDURE [dbo].[usp_cmw_divisions_insert]
(
	@id		Int		OUTPUT,
	@name		VarChar(50)
)
AS
INSERT INTO cmw_divisions
(
	name
)
VALUES
(
	@name
)

SELECT
	@id = SCOPE_IDENTITY()


