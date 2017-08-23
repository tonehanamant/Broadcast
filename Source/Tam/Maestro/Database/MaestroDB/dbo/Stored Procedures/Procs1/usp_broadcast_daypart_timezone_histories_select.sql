
CREATE PROCEDURE [dbo].[usp_broadcast_daypart_timezone_histories_select]
(
	@id Int
)
AS
SELECT
	*
FROM
	broadcast_daypart_timezone_histories WITH(NOLOCK)
WHERE
	id = @id

