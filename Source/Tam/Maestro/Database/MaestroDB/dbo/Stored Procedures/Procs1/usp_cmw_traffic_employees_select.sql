CREATE PROCEDURE usp_cmw_traffic_employees_select
(
	@cmw_traffic_id		Int,
	@employee_id		Int,
	@effective_date		DateTime
)
AS
SELECT
	*
FROM
	cmw_traffic_employees WITH(NOLOCK)
WHERE
	cmw_traffic_id=@cmw_traffic_id
	AND
	employee_id=@employee_id
	AND
	effective_date=@effective_date

