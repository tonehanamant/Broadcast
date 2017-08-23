CREATE PROCEDURE usp_outlook_exports_update
(
	@id		Int,
	@employee_id		Int,
	@run_number		SmallInt,
	@date_created		DateTime
)
AS
UPDATE outlook_exports SET
	employee_id = @employee_id,
	run_number = @run_number,
	date_created = @date_created
WHERE
	id = @id

