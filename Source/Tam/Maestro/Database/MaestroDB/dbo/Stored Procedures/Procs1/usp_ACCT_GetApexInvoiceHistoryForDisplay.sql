

-- =============================================
-- Author:		John Carsley
-- Create date: 05/01/2012
-- Description:	Gets receivable invoice universe records for Apex by to and from date
-- Usage: exec dbo.usp_ACCT_GetApexInvoiceHistoryForDisplay 6, 2011, 6, 2011
-- =============================================
CREATE PROCEDURE [dbo].[usp_ACCT_GetApexInvoiceHistoryForDisplay]
	 @from_month int
	,@from_year int 
	,@to_month int
	,@to_year int
AS
BEGIN
	SET NOCOUNT ON;
	declare @from_date smalldatetime
	declare @to_date smalldatetime
	
	set @from_date = convert(smalldatetime, convert(varchar(2), @from_month) + '/1/' + convert(varchar(4), @from_year))
	set @to_date = convert(smalldatetime, convert(varchar(2), @to_month) + '/1/' + convert(varchar(4), @to_year))
	
	create table #media_months (id int)

	insert into #media_months
	Select id
	from media_months mm  WITH (NOLOCK)
	where convert(smalldatetime, convert(varchar(2), mm.month) + '/1/' + convert(varchar(4), mm.year)) 
		between @from_date and @to_date
		
	SELECT
		   ri.receivable_invoice_id
		  ,ri.start_date
		  ,ri.end_date
		  ,ri.media_month_id
		  ,ri.entity_id
		  ,product_id = pr.id
		  ,advertiser = adv.name
		  ,agency = agy.name
		  ,product = pr.name
		  ,ri.customer_number
		  ,gp.customer_name
		  ,ri.invoice_number
		  ,ri.special_notes
		  ,ri.total_units
		  ,ri.total_due_gross
		  ,ri.total_due_net
		  ,ri.total_credits
		  ,ri.document_id
		  ,ri.is_mailed
		  ,ri.ISCI_codes
		  ,ri.active
		  ,ri.date_created
		  ,ri.date_modified
		  ,ri.modified_by
		  ,cmw_invoice_id = ci.id
		  ,cmw_traffic_id = ct.id
		  ,network_id = n.id
		  ,network_code = n.code
		  ,network_name = n.name
	  INTO #invoice_display
	  FROM uvw_receivable_invoice_universe ri  WITH (NOLOCK)
	  JOIN cmw_invoices ci WITH (NOLOCK) ON ci.id = ri.entity_id
	  JOIN cmw_invoice_details cid WITH (NOLOCK) ON cid.cmw_invoice_id = ci.id
	  JOIN cmw_traffic ct WITH (NOLOCK) ON ct.id = cid.cmw_traffic_id
	  JOIN networks n on ct.network_id = n.id
	  JOIN great_plains_customers gp WITH (NOLOCK) on ri.customer_number = gp.customer_number
	  LEFT JOIN cmw_traffic_products pr WITH (NOLOCK) ON pr.id = ct.cmw_traffic_product_id 
	  LEFT JOIN cmw_traffic_companies adv WITH (NOLOCK) ON adv.id = ct.advertiser_cmw_traffic_company_id
	  LEFT JOIN cmw_traffic_companies agy WITH (NOLOCK) ON agy.id = ct.agency_cmw_traffic_company_id
	  WHERE exists
		(SELECT 1
			FROM #media_months mm 
			WHERE ri.media_month_id = mm.id
		)
	  AND ri.invoice_type_id = 2 --Apex Invoices
	  ORDER BY ri.invoice_number
  
	SELECT DISTINCT
		   receivable_invoice_id
		  ,start_date
		  ,end_date
		  ,media_month_id
		  ,entity_id
		  ,product_id
		  ,advertiser
		  ,agency
		  ,product
		  ,customer_number
		  ,customer_name
		  ,invoice_number
		  ,special_notes
		  ,total_units
		  ,total_due_gross
		  ,total_due_net
		  ,total_credits
		  ,document_id
		  ,is_mailed
		  ,ISCI_codes
		  ,active
		  ,date_created
		  ,date_modified
		  ,modified_by
	FROM #invoice_display	
	
	--order ids
	SELECT 
		cmw_invoice_id, 
		cmw_traffic_id
	FROM #invoice_display inv
	
	--networks
    SELECT DISTINCT
		cmw_invoice_id,
		network_id,
		network_name,
		network_code
	FROM #invoice_display inv 
	
	--flights
	SELECT
		inv.cmw_invoice_id, 
		inv.cmw_traffic_id,
		ctf.start_date, 
		ctf.end_date, 
		ctf.selected
	from #invoice_display inv
	join cmw_traffic_flights ctf on inv.cmw_traffic_id = ctf.cmw_traffic_id
	order by inv.cmw_invoice_id, inv.cmw_traffic_id
 END
