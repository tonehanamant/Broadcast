-- =============================================
-- Author:		John Carsley
-- Create date: 02/22/2012
-- Description:	Used by Accounting Composer
-- USAGE usp_ACCT_GetApexInvoiceItems 1, 2011, 1, 2011
-- =============================================
CREATE PROCEDURE [dbo].[usp_ACCT_GetApexInvoiceItems](
		 @from_month int
		,@from_year int 
		,@to_month int
		,@to_year int)
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
		
	CREATE TABLE #filtered_cmw_traffic (cmw_invoice_id INT, cmw_traffic_id INT, max_version_number INT)
	INSERT INTO #filtered_cmw_traffic
		SELECT
			ct.cmw_invoice_id,
			ct.cmw_traffic_id ,
			MAX(ct.version_number) 'max_version_number'
		FROM (
			SELECT 
				ci.id 'cmw_invoice_id',
				ISNULL(ct.original_cmw_traffic_id,ct.id) 'cmw_traffic_id',
				ct.version_number
			FROM
				cmw_traffic ct (NOLOCK)
				JOIN cmw_invoice_details cid (NOLOCK) ON cid.cmw_traffic_id=ISNULL(ct.original_cmw_traffic_id,ct.id)
				JOIN cmw_invoices ci on ci.id = cid.cmw_invoice_id
				JOIN #media_months mm on ci.media_month_id = mm.id
		) ct
		GROUP BY
			ct.cmw_invoice_id,
			ct.cmw_traffic_id
		
	CREATE TABLE #units_per_month (media_month_id INT, cmw_traffic_detail_id INT, total_weeks INT)
	INSERT INTO #units_per_month
		SELECT
		
			mm.id,
			ctd.id,
			COUNT(*) 
		FROM
			#filtered_cmw_traffic fct
			JOIN cmw_traffic ct				(NOLOCK) ON ISNULL(ct.original_cmw_traffic_id,ct.id)=fct.cmw_traffic_id AND ct.version_number=fct.max_version_number
			JOIN cmw_traffic_details ctd	(NOLOCK) ON ctd.cmw_traffic_id=ct.id
			JOIN cmw_traffic_flights ctf	(NOLOCK) ON ctf.cmw_traffic_id=ctd.cmw_traffic_id AND (ctf.start_date <= ISNULL(ctd.suspend_date, ctd.end_date) AND ctf.end_date >= ctd.start_date)
			JOIN media_months mm			(NOLOCK) ON (mm.start_date <= ctf.end_date AND mm.end_date >= ctf.start_date)
			JOIN #media_months tmp					 ON mm.id = tmp.id
		GROUP BY
			mm.id,
			ctd.id
	
	CREATE TABLE #units (cmw_traffic_detail_id INT, total_weeks INT)
	INSERT INTO #units
		SELECT
			ctd.id,
			COUNT(*)
		FROM 
			#filtered_cmw_traffic fct
			JOIN cmw_traffic ct				(NOLOCK) ON ISNULL(ct.original_cmw_traffic_id,ct.id)=fct.cmw_traffic_id AND ct.version_number=fct.max_version_number
			JOIN cmw_traffic_details ctd	(NOLOCK) ON ctd.cmw_traffic_id=ct.id
			JOIN cmw_traffic_flights ctf	(NOLOCK) ON ctf.cmw_traffic_id=ctd.cmw_traffic_id AND (ctf.start_date <= ISNULL(ctd.suspend_date, ctd.end_date) AND ctf.end_date >= ctd.start_date)
		GROUP BY
			ctd.id
	
	SELECT DISTINCT
		fct.cmw_invoice_id,
		ci.external_invoice_number,
		mm.id as 'media_month_id',
		agy.name as 'agency',
		ISNULL(map.advertiser_alias, adv.name) as 'advertiser',
		COALESCE(map.product_alias, ctp.name, adv.name) as 'product',
		ctp.id as 'product_id',
		ISNULL(e.firstname + ' ' + e.lastname,'') as 'salesperson',
		ci.gross_due as 'gross',
		ci.external_invoice_number as 'invoice number',
		cust.customer_number,
		cust.customer_name,
		--Orders = (SELECT CAST( fct.cmw_traffic_id As VARCHAR) + ','
		--			from #filtered_cmw_traffic fct1
		--			where fct.cmw_invoice_id = fct1.cmw_invoice_id
		--			FOR XML PATH('')),
		--SUM(CASE WHEN ctd.unit_cost = 0 THEN CAST(ctd.total_units AS FLOAT) / CAST(u.total_weeks AS FLOAT) * upm.total_weeks ELSE 0 END) as 'bonus units in month',
		--SUM((CAST(ctd.total_units AS FLOAT) / CAST(u.total_weeks AS FLOAT)) * upm.total_weeks) as 'units in month',
		SUM(((CAST(ctd.total_units AS MONEY) / CAST(u.total_weeks AS MONEY)) * upm.total_weeks) * ctd.unit_cost) as 'contracted'
	FROM
		#filtered_cmw_traffic fct
		JOIN cmw_invoices ci			WITH (NOLOCK) ON fct.cmw_invoice_id = ci.id
		JOIN cmw_traffic ct				WITH (NOLOCK) ON ISNULL(ct.original_cmw_traffic_id,ct.id)=fct.cmw_traffic_id AND ct.version_number=fct.max_version_number
		JOIN cmw_traffic_details ctd	WITH (NOLOCK) ON ctd.cmw_traffic_id=ct.id
		JOIN media_months mm			WITH (NOLOCK) ON (mm.start_date <= ctd.end_date AND mm.end_date >= ctd.start_date)
		JOIN #units_per_month upm				 ON upm.cmw_traffic_detail_id=ctd.id AND mm.id=upm.media_month_id
		JOIN #units u							 ON u.cmw_traffic_detail_id=ctd.id
		LEFT JOIN cmw_traffic_companies adv WITH (NOLOCK) on ct.advertiser_cmw_traffic_company_id = adv.id
		LEFT JOIN cmw_traffic_companies agy WITH (NOLOCK)on ct.agency_cmw_traffic_company_id = agy.id
		LEFT JOIN cmw_traffic_products ctp WITH (NOLOCK)on ct.cmw_traffic_product_id = ctp.id
		LEFT JOIN employees e WITH (NOLOCK) on ct.salesperson_employee_id = e.id
		LEFT JOIN great_plains_mapping map WITH (NOLOCK) ON map.product_id = ct.cmw_traffic_product_id
		LEFT JOIN great_plains_customers cust WITH (NOLOCK) ON cust.customer_number = map.great_plains_customer_number
	GROUP BY
		fct.cmw_invoice_id,
		ci.external_invoice_number,
		mm.id,
		agy.name,
		ISNULL(map.advertiser_alias, adv.name),
		COALESCE(map.product_alias, ctp.name, adv.name),
		ctp.id,
		ISNULL(e.firstname + ' ' + e.lastname,''),
		ci.gross_due,
		ci.external_invoice_number,
		cust.customer_number,
		cust.customer_name
	ORDER BY cmw_invoice_id
	
	--order ids
	SELECT cmw_invoice_id, cmw_traffic_id, ct.flight_text
	from #filtered_cmw_traffic fct
		join cmw_traffic ct on fct.cmw_traffic_id = ct.id
	order by cmw_invoice_id, cmw_traffic_id
	
	--networks
	SELECT
		fct.cmw_invoice_id, 
		fct.cmw_traffic_id,
		n.name as 'network',
		n.code as 'network code'
	from #filtered_cmw_traffic fct
	join cmw_traffic ct on fct.cmw_traffic_id = ct.id
	join networks n on n.id = ct.network_id
	order by fct.cmw_invoice_id, fct.cmw_traffic_id

	--flights
	SELECT
		fct.cmw_invoice_id, 
		fct.cmw_traffic_id,
		fct.cmw_traffic_id,
		ctf.start_date, 
		ctf.end_date, 
		ctf.selected
	from #filtered_cmw_traffic fct
	join cmw_traffic_flights ctf on fct.cmw_traffic_id = ctf.cmw_traffic_id
	order by fct.cmw_invoice_id, fct.cmw_traffic_id
	
	--no need to drop tables, it's more efficient to let the database do it.
END
