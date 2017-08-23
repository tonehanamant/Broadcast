CREATE PROCEDURE [dbo].[usp_ACCT_GetMediaMonthsAssociatedWithCmwInvoices]
AS
select 
	distinct mm.id, 
	mm.year, 
	mm.month, 
	mm.media_month, 
	mm.start_date, 
	mm.end_date 
from
	cmw_invoices ci WITH (NOLOCK)
	join media_months mm WITH (NOLOCK) on 
		ci.media_month_id = mm.id
order by
	mm.id
