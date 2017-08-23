
CREATE PROCEDURE [dbo].[usp_receivable_invoices_insert]
(
	@id		Int		OUTPUT,
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
INSERT INTO receivable_invoices
(
	media_month_id,
	entity_id,
	customer_number,
	invoice_number,
	special_notes,
	total_units,
	total_due_gross,
	total_due_net,
	total_credits,
	document_id,
	is_mailed,
	ISCI_codes,
	invoice_type_id,
	active,
	effective_date,
	date_created,
	date_modified,
	modified_by
)
VALUES
(
	@media_month_id,
	@entity_id,
	@customer_number,
	@invoice_number,
	@special_notes,
	@total_units,
	@total_due_gross,
	@total_due_net,
	@total_credits,
	@document_id,
	@is_mailed,
	@ISCI_codes,
	@invoice_type_id,
	@active,
	@effective_date,
	@date_created,
	@date_modified,
	@modified_by
)

SELECT
	@id = SCOPE_IDENTITY()


