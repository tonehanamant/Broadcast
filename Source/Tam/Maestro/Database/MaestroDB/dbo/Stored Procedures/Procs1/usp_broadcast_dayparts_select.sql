
CREATE PROCEDURE [dbo].[usp_broadcast_dayparts_select]
(
	@id Int
)
AS
SELECT
	*
FROM
	broadcast_dayparts WITH(NOLOCK)
WHERE
	id = @id

