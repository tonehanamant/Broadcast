
CREATE PROCEDURE [dbo].[usp_broadcast_daypart_timezones_update]
(
	@id		Int,
	@broadcast_daypart_id		Int,
	@timezone_id		Int,
	@daypart_id		Int,
	@effective_date		DateTime
)
AS
UPDATE broadcast_daypart_timezones SET
	broadcast_daypart_id = @broadcast_daypart_id,
	timezone_id = @timezone_id,
	daypart_id = @daypart_id,
	effective_date = @effective_date
WHERE
	id = @id

