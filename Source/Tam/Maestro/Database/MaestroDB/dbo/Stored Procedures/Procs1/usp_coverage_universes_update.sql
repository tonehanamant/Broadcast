CREATE PROCEDURE usp_coverage_universes_update
(
	@id		Int,
	@base_media_month_id		Int,
	@sales_model_id		Int,
	@approved_by_employee_id		Int,
	@date_approved		DateTime,
	@date_created		DateTime,
	@date_last_modified		DateTime
)
AS
UPDATE coverage_universes SET
	base_media_month_id = @base_media_month_id,
	sales_model_id = @sales_model_id,
	approved_by_employee_id = @approved_by_employee_id,
	date_approved = @date_approved,
	date_created = @date_created,
	date_last_modified = @date_last_modified
WHERE
	id = @id

