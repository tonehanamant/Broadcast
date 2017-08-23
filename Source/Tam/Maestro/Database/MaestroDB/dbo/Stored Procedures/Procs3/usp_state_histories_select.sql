CREATE PROCEDURE usp_state_histories_select
(
	@state_id		Int,
	@start_date		DateTime
)
AS
SELECT
	*
FROM
	state_histories WITH(NOLOCK)
WHERE
	state_id=@state_id
	AND
	start_date=@start_date

