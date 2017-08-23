CREATE PROCEDURE usp_cmw_invoice_adjustments_insert
(
	@id		Int		OUTPUT,
	@cmw_invoice_id		Int,
	@applies_to_media_month_id		Int,
	@status_code		TinyInt,
	@amount		Money,
	@description		VarChar(2047),
	@created_by_employee_id		Int,
	@modified_by_employee_id		Int,
	@date_created		DateTime,
	@date_last_modified		DateTime
)
AS
INSERT INTO cmw_invoice_adjustments
(
	cmw_invoice_id,
	applies_to_media_month_id,
	status_code,
	amount,
	description,
	created_by_employee_id,
	modified_by_employee_id,
	date_created,
	date_last_modified
)
VALUES
(
	@cmw_invoice_id,
	@applies_to_media_month_id,
	@status_code,
	@amount,
	@description,
	@created_by_employee_id,
	@modified_by_employee_id,
	@date_created,
	@date_last_modified
)

SELECT
	@id = SCOPE_IDENTITY()

