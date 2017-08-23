
CREATE PROCEDURE [dbo].[usp_broadcast_daypart_timezones_select_all]
AS
SELECT
	*
FROM
	broadcast_daypart_timezones WITH(NOLOCK)

