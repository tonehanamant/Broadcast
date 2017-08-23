
CREATE PROCEDURE [dbo].[usp_broadcast_dayparts_select_all]
AS
SELECT
	*
FROM
	broadcast_dayparts WITH(NOLOCK)

