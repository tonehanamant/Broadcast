CREATE PROCEDURE usp_cmw_invoices_insert
(
	@id		Int		OUTPUT,
	@media_month_id		Int,
	@status_code		TinyInt,
	@external_invoice_number		VarChar(63),
	@gross_due		Money,
	@agency_commission_fee		Money,
	@net_due_to_rep		Money,
	@rep_commission_fee		Money,
	@net_due_to_network		Money,
	@created_by_employee_id		Int,
	@modified_by_employee_id		Int,
	@date_created		DateTime,
	@date_last_modified		DateTime
)
AS
INSERT INTO cmw_invoices
(
	media_month_id,
	status_code,
	external_invoice_number,
	gross_due,
	agency_commission_fee,
	net_due_to_rep,
	rep_commission_fee,
	net_due_to_network,
	created_by_employee_id,
	modified_by_employee_id,
	date_created,
	date_last_modified
)
VALUES
(
	@media_month_id,
	@status_code,
	@external_invoice_number,
	@gross_due,
	@agency_commission_fee,
	@net_due_to_rep,
	@rep_commission_fee,
	@net_due_to_network,
	@created_by_employee_id,
	@modified_by_employee_id,
	@date_created,
	@date_last_modified
)

SELECT
	@id = SCOPE_IDENTITY()

