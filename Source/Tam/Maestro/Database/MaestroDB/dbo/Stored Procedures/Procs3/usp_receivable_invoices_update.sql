
CREATE PROCEDURE [dbo].[usp_receivable_invoices_update]
(
	@id		Int,
	@media_month_id		Int,
	@entity_id		Int,
	@customer_number		Char(8),
	@invoice_number		VarChar(63),
	@special_notes		VARCHAR(MAX),
	@total_units		Int,
	@total_due_gross		Money,
	@total_due_net		Money,
	@total_credits		Money,
	@document_id		Int,
	@is_mailed		Bit,
	@ISCI_codes		VarChar(100),
	@invoice_type_id		Int,
	@active		Bit,
	@effective_date		DateTime,
	@date_created		DateTime,
	@date_modified		DateTime,
	@modified_by		VarChar(50)
)
AS
UPDATE receivable_invoices SET
	media_month_id = @media_month_id,
	entity_id = @entity_id,
	customer_number = @customer_number,
	invoice_number = @invoice_number,
	special_notes = @special_notes,
	total_units = @total_units,
	total_due_gross = @total_due_gross,
	total_due_net = @total_due_net,
	total_credits = @total_credits,
	document_id = @document_id,
	is_mailed = @is_mailed,
	ISCI_codes = @ISCI_codes,
	invoice_type_id = @invoice_type_id,
	active = @active,
	effective_date = @effective_date,
	date_created = @date_created,
	date_modified = @date_modified,
	modified_by = @modified_by
WHERE
	id = @id


