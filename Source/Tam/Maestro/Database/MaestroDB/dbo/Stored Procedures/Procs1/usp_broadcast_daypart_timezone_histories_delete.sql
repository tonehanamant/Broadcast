
CREATE PROCEDURE [dbo].[usp_broadcast_daypart_timezone_histories_delete]
(
	@id Int
)
AS
DELETE FROM broadcast_daypart_timezone_histories WHERE id=@id

