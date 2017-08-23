


CREATE Procedure [dbo].[usp_REC_GetMSOLostOpportunityForMonthInvoiceDollars]
(
	@media_month_id int,
	@business_id int
)

AS

DECLARE @effective_date DATETIME;
SET @effective_date = (select media_months.end_date from media_months where id = @media_month_id);

select sum(invoices.invoice_gross_due / 100.0)
		from invoices (NOLOCK) 
		join systems (NOLOCK) on systems.id = invoices.system_id
	where invoices.media_month_id = @media_month_id and 
	invoices.system_id in
	(
		select system_id from GetAllSystemsAssociatedToMSO(@business_id, @effective_date)
    )
	



