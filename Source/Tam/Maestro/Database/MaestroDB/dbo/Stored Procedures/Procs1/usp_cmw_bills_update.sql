CREATE PROCEDURE usp_cmw_bills_update
(
	@id		Int,
	@cmw_traffic_id		Int,
	@media_month_id		Int,
	@billing_terms_id		Int,
	@document_id		Int,
	@customer_number		Char(4),
	@invoice_date		DateTime,
	@cmw_invoice_number		VarChar(63),
	@cmw_invoice_ordinal		SmallInt,
	@total_gross_due		Money,
	@total_net_due		Money,
	@sub_total		Money,
	@payments_credits		Money,
	@balance_due		Money,
	@date_created		DateTime
)
AS
UPDATE cmw_bills SET
	cmw_traffic_id = @cmw_traffic_id,
	media_month_id = @media_month_id,
	billing_terms_id = @billing_terms_id,
	document_id = @document_id,
	customer_number = @customer_number,
	invoice_date = @invoice_date,
	cmw_invoice_number = @cmw_invoice_number,
	cmw_invoice_ordinal = @cmw_invoice_ordinal,
	total_gross_due = @total_gross_due,
	total_net_due = @total_net_due,
	sub_total = @sub_total,
	payments_credits = @payments_credits,
	balance_due = @balance_due,
	date_created = @date_created
WHERE
	id = @id

