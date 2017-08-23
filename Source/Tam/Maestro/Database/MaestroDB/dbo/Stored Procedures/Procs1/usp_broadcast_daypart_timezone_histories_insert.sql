
CREATE PROCEDURE [dbo].[usp_broadcast_daypart_timezone_histories_insert]
(
	@id		Int		OUTPUT,
	@broadcast_daypart_id		Int,
	@timezone_id		Int,
	@daypart_id		Int,
	@start_date		DateTime,
	@end_date		DateTime
)
AS
INSERT INTO broadcast_daypart_timezone_histories
(
	broadcast_daypart_id,
	timezone_id,
	daypart_id,
	start_date,
	end_date
)
VALUES
(
	@broadcast_daypart_id,
	@timezone_id,
	@daypart_id,
	@start_date,
	@end_date
)

SELECT
	@id = SCOPE_IDENTITY()


