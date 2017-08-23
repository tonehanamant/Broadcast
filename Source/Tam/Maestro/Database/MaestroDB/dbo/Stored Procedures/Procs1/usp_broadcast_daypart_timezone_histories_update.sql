
CREATE PROCEDURE [dbo].[usp_broadcast_daypart_timezone_histories_update]
(
	@id		Int,
	@broadcast_daypart_id		Int,
	@timezone_id		Int,
	@daypart_id		Int,
	@start_date		DateTime,
	@end_date		DateTime
)
AS
UPDATE broadcast_daypart_timezone_histories SET
	broadcast_daypart_id = @broadcast_daypart_id,
	timezone_id = @timezone_id,
	daypart_id = @daypart_id,
	start_date = @start_date,
	end_date = @end_date
WHERE
	id = @id


