
CREATE PROCEDURE [dbo].[usp_broadcast_daypart_timezone_histories_select_all]
AS
SELECT
	*
FROM
	broadcast_daypart_timezone_histories WITH(NOLOCK)

