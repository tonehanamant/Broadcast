CREATE PROCEDURE usp_traffic_employees_select
(
	@traffic_id		Int,
	@employee_id		Int,
	@effective_date		DateTime
)
AS
SELECT
	*
FROM
	traffic_employees WITH(NOLOCK)
WHERE
	traffic_id=@traffic_id
	AND
	employee_id=@employee_id
	AND
	effective_date=@effective_date

