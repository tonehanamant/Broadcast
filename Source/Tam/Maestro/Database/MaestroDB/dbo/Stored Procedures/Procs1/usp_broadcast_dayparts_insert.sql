
CREATE PROCEDURE [dbo].[usp_broadcast_dayparts_insert]
(
	@id		Int		OUTPUT,
	@name		VarChar(63),
	@description		VarChar(127),
	@map_set		VarChar(63)
)
AS
INSERT INTO broadcast_dayparts
(
	name,
	description,
	map_set
)
VALUES
(
	@name,
	@description,
	@map_set
)

SELECT
	@id = SCOPE_IDENTITY()

