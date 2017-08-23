
-- exec usp_ACCT_GetReceivableInvoiceByDate 31034, 359, '08/17/2011 1:00 PM'
CREATE PROCEDURE [dbo].[usp_ACCT_GetReceivableInvoiceByDate]
(
	 @entity_id int
	,@media_month_id int
	,@effective_date datetime
)
AS
BEGIN

SELECT receivable_invoice_id
	, media_month_id
	, entity_id
	, customer_number
	, invoice_number
	, special_notes
	, total_units
	, total_due_gross
	, total_due_net
	, total_credits
	, document_id
	, is_mailed
	, ISCI_codes
	,invoice_type_id
	, active
	, date_created
	, date_modified
	, modified_by
	, start_date
	, end_date
FROM 
	[dbo].[uvw_receivable_invoice_universe] vw WITH (NOLOCK)
WHERE
	[entity_id] = @entity_id
	AND [media_month_id] = @media_month_id
	AND
		(vw.start_date <= @effective_date
		AND
			(vw.end_date >= @effective_date
			OR vw.end_date IS NULL))
END
