CREATE PROCEDURE usp_outlook_exports_insert
(
	@id		Int		OUTPUT,
	@employee_id		Int,
	@run_number		SmallInt,
	@date_created		DateTime
)
AS
INSERT INTO outlook_exports
(
	employee_id,
	run_number,
	date_created
)
VALUES
(
	@employee_id,
	@run_number,
	@date_created
)

SELECT
	@id = SCOPE_IDENTITY()

