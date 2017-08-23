
CREATE PROCEDURE [dbo].[usp_broadcast_daypart_timezones_delete]
(
	@id Int
)
AS
DELETE FROM broadcast_daypart_timezones WHERE id=@id

