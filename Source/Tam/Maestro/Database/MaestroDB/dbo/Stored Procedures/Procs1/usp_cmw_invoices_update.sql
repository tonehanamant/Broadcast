CREATE PROCEDURE usp_cmw_invoices_update
(
	@id		Int,
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
UPDATE cmw_invoices SET
	media_month_id = @media_month_id,
	status_code = @status_code,
	external_invoice_number = @external_invoice_number,
	gross_due = @gross_due,
	agency_commission_fee = @agency_commission_fee,
	net_due_to_rep = @net_due_to_rep,
	rep_commission_fee = @rep_commission_fee,
	net_due_to_network = @net_due_to_network,
	created_by_employee_id = @created_by_employee_id,
	modified_by_employee_id = @modified_by_employee_id,
	date_created = @date_created,
	date_last_modified = @date_last_modified
WHERE
	id = @id

