CREATE PROCEDURE usp_system_dayparts_delete
(
	@system_id		Int,
	@daypart_id		Int
)
AS
DELETE FROM system_dayparts WHERE system_id=@system_id AND daypart_id=@daypart_id
