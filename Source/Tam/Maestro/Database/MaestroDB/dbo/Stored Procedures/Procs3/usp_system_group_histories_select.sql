CREATE PROCEDURE usp_system_group_histories_select
(
	@system_group_id		Int,
	@start_date		DateTime
)
AS
SELECT
	*
FROM
	system_group_histories WITH(NOLOCK)
WHERE
	system_group_id=@system_group_id
	AND
	start_date=@start_date

