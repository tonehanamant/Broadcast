

CREATE Procedure [dbo].[usp_REC_GetMSOLostOpportunityForYearBySystemInvoiceDollars]
(
	@year int,
	@business_id int
)

AS

DECLARE @effective_date DATETIME;
SET @effective_date = cast(cast(@year as varchar) + '-12-31' as datetime);

select systems.code, invoices.system_id, sum(invoices.invoice_gross_due / 100.0)
		from invoices (NOLOCK) 
		join systems (NOLOCK) on systems.id = invoices.system_id 
		join media_months (NOLOCK) on media_months.id = invoices.media_month_id
	where media_months.year = @year and 
	invoices.system_id in
	(
		select system_id from GetAllSystemsAssociatedToMSO(@business_id, @effective_date)
    )
	group by systems.code, invoices.system_id
