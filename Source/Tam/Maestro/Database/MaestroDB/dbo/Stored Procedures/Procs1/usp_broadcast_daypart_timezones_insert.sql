
CREATE PROCEDURE [dbo].[usp_broadcast_daypart_timezones_insert]
(
	@id		Int		OUTPUT,
	@broadcast_daypart_id		Int,
	@timezone_id		Int,
	@daypart_id		Int,
	@effective_date		DateTime
)
AS
INSERT INTO broadcast_daypart_timezones
(
	broadcast_daypart_id,
	timezone_id,
	daypart_id,
	effective_date
)
VALUES
(
	@broadcast_daypart_id,
	@timezone_id,
	@daypart_id,
	@effective_date
)

SELECT
	@id = SCOPE_IDENTITY()


