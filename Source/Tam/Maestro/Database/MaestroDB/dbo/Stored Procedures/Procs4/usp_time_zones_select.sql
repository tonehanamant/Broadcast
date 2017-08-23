

CREATE PROCEDURE [dbo].[usp_time_zones_select]
(
	@id Int
)
AS
SELECT
	*
FROM
	time_zones WITH(NOLOCK)
WHERE
	id = @id

