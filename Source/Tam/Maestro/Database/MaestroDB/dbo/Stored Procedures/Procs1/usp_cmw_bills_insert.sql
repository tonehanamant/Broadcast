CREATE PROCEDURE usp_cmw_bills_insert
(
	@id		Int		OUTPUT,
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
INSERT INTO cmw_bills
(
	cmw_traffic_id,
	media_month_id,
	billing_terms_id,
	document_id,
	customer_number,
	invoice_date,
	cmw_invoice_number,
	cmw_invoice_ordinal,
	total_gross_due,
	total_net_due,
	sub_total,
	payments_credits,
	balance_due,
	date_created
)
VALUES
(
	@cmw_traffic_id,
	@media_month_id,
	@billing_terms_id,
	@document_id,
	@customer_number,
	@invoice_date,
	@cmw_invoice_number,
	@cmw_invoice_ordinal,
	@total_gross_due,
	@total_net_due,
	@sub_total,
	@payments_credits,
	@balance_due,
	@date_created
)

SELECT
	@id = SCOPE_IDENTITY()

