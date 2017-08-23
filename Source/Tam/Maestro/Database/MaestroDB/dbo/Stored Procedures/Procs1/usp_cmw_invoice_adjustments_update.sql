CREATE PROCEDURE usp_cmw_invoice_adjustments_update
(
	@id		Int,
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
UPDATE cmw_invoice_adjustments SET
	cmw_invoice_id = @cmw_invoice_id,
	applies_to_media_month_id = @applies_to_media_month_id,
	status_code = @status_code,
	amount = @amount,
	description = @description,
	created_by_employee_id = @created_by_employee_id,
	modified_by_employee_id = @modified_by_employee_id,
	date_created = @date_created,
	date_last_modified = @date_last_modified
WHERE
	id = @id

