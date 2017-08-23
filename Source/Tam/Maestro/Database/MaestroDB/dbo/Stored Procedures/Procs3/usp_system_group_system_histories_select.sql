CREATE PROCEDURE usp_system_group_system_histories_select
(
	@system_group_id		Int,
	@system_id		Int,
	@start_date		DateTime
)
AS
SELECT
	*
FROM
	system_group_system_histories WITH(NOLOCK)
WHERE
	system_group_id=@system_group_id
	AND
	system_id=@system_id
	AND
	start_date=@start_date

