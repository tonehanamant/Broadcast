CREATE PROCEDURE usp_system_dayparts_select
(
	@system_id		Int,
	@daypart_id		Int
)
AS
SELECT
	*
FROM
	system_dayparts WITH(NOLOCK)
WHERE
	system_id=@system_id
	AND
	daypart_id=@daypart_id

