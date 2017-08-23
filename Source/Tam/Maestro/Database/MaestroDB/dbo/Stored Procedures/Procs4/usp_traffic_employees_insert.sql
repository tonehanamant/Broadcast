CREATE PROCEDURE usp_traffic_employees_insert
(
	@traffic_id		Int,
	@employee_id		Int,
	@effective_date		DateTime
)
AS
INSERT INTO traffic_employees
(
	traffic_id,
	employee_id,
	effective_date
)
VALUES
(
	@traffic_id,
	@employee_id,
	@effective_date
)

