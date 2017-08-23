
CREATE PROCEDURE [dbo].[usp_broadcast_daypart_timezones_select]
(
	@id Int
)
AS
SELECT
	*
FROM
	broadcast_daypart_timezones WITH(NOLOCK)
WHERE
	id = @id

