CREATE PROCEDURE usp_coverage_universes_insert
(
	@id		Int		OUTPUT,
	@base_media_month_id		Int,
	@sales_model_id		Int,
	@approved_by_employee_id		Int,
	@date_approved		DateTime,
	@date_created		DateTime,
	@date_last_modified		DateTime
)
AS
INSERT INTO coverage_universes
(
	base_media_month_id,
	sales_model_id,
	approved_by_employee_id,
	date_approved,
	date_created,
	date_last_modified
)
VALUES
(
	@base_media_month_id,
	@sales_model_id,
	@approved_by_employee_id,
	@date_approved,
	@date_created,
	@date_last_modified
)

SELECT
	@id = SCOPE_IDENTITY()

