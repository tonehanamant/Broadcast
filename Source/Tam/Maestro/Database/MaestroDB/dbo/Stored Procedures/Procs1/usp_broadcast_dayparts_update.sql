
CREATE PROCEDURE [dbo].[usp_broadcast_dayparts_update]
(
	@id		Int,
	@name		VarChar(63),
	@description		VarChar(127),
	@map_set		VarChar(63)
)
AS
UPDATE broadcast_dayparts SET
	name = @name,
	description = @description,
	map_set = @map_set
WHERE
	id = @id

