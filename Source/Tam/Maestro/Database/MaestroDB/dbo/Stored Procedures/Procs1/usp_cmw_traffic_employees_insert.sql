CREATE PROCEDURE usp_cmw_traffic_employees_insert
(
	@cmw_traffic_id		Int,
	@employee_id		Int,
	@effective_date		DateTime
)
AS
INSERT INTO cmw_traffic_employees
(
	cmw_traffic_id,
	employee_id,
	effective_date
)
VALUES
(
	@cmw_traffic_id,
	@employee_id,
	@effective_date
)

