
CREATE PROCEDURE [dbo].[usp_time_zones_select_all]
AS
SELECT
	*
FROM
	time_zones WITH(NOLOCK)

