-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 10/21/2011
-- Description:	<Description,,>
-- =============================================
-- usp_BRS_GetInvoiceBusinessObject 4
CREATE PROCEDURE [dbo].[usp_BRS_GetInvoiceBusinessObject]
	@cmw_invoice_id INT
AS
BEGIN
	DECLARE @media_month_id INT
	SELECT @media_month_id = ci.media_month_id FROM cmw_invoices ci (NOLOCK) WHERE ci.id=@cmw_invoice_id
	
	CREATE TABLE #filtered_cmw_traffic (cmw_traffic_id INT, max_version_number INT)
	INSERT INTO #filtered_cmw_traffic
		SELECT
			ct.id 'cmw_traffic_id',
			MAX(ct.version_number) 'max_version_number'
		FROM (
			SELECT 
				ISNULL(ct.original_cmw_traffic_id,ct.id) 'id',
				ct.version_number
			FROM
				cmw_traffic ct (NOLOCK)
				JOIN cmw_invoice_details cid (NOLOCK) ON cid.cmw_traffic_id=ISNULL(ct.original_cmw_traffic_id,ct.id)
					AND cid.cmw_invoice_id=@cmw_invoice_id
		) ct
		GROUP BY
			ct.id
			
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
			JOIN media_months mm			(NOLOCK) ON (mm.start_date <= ctf.end_date AND mm.end_date >= ctf.start_date) AND mm.id=@media_month_id
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
			
	-- cmw_invoices
	SELECT
		mm.*,
		ci.*
	FROM
		cmw_invoices ci			(NOLOCK)
		JOIN media_months mm	(NOLOCK) ON mm.id=ci.media_month_id
	WHERE
		ci.id=@cmw_invoice_id
			
				
	-- cmw_invoice_details
	SELECT
		cid.*
	FROM
		cmw_invoice_details cid (NOLOCK)
	WHERE
		cid.cmw_invoice_id=@cmw_invoice_id
		
		
	-- cmw_invoice_adjustments
	SELECT
		mm.*,
		cia.*
	FROM
		cmw_invoice_adjustments cia (NOLOCK)
		JOIN media_months mm	(NOLOCK) ON mm.id=cia.applies_to_media_month_id
	WHERE
		cia.cmw_invoice_id=@cmw_invoice_id
		
	-- total monthly revenue
	SELECT
		mm.id 'media_month_id',
		AVG(ctd.unit_cost) 'avg unit cost',
		CAST(AVG(ctd.unit_cost / sl.delivery_multiplier) AS MONEY) 'avg unit cost equivalized (:30)',
		SUM(CASE WHEN ctd.unit_cost = 0 THEN CAST(ctd.total_units AS FLOAT) / CAST(u.total_weeks AS FLOAT) * upm.total_weeks ELSE 0 END) 'bonus units in month',
		SUM((CAST(ctd.total_units AS FLOAT) / CAST(u.total_weeks AS FLOAT)) * upm.total_weeks) 'units in month',
		SUM(((CAST(ctd.total_units AS MONEY) / CAST(u.total_weeks AS MONEY)) * upm.total_weeks) * ctd.unit_cost) 'revenue'
	FROM
		#filtered_cmw_traffic fct
		JOIN cmw_traffic ct				(NOLOCK) ON ISNULL(ct.original_cmw_traffic_id,ct.id)=fct.cmw_traffic_id AND ct.version_number=fct.max_version_number
		JOIN spot_lengths sl			(NOLOCK) ON sl.id=ct.default_spot_length_id
		JOIN cmw_traffic_details ctd	(NOLOCK) ON ctd.cmw_traffic_id=ct.id
		JOIN media_months mm			(NOLOCK) ON (mm.start_date <= ctd.end_date AND mm.end_date >= ctd.start_date)
		JOIN #units_per_month upm				 ON upm.cmw_traffic_detail_id=ctd.id AND mm.id=upm.media_month_id AND mm.id=@media_month_id
		JOIN #units u							 ON u.cmw_traffic_detail_id=ctd.id
	GROUP BY
		mm.id,
		mm.media_month
	ORDER BY
		mm.media_month
	
	
	-- cmw_traffic (order information, could return more than one)
	SELECT DISTINCT
		ct.id,
		CASE WHEN ct.original_cmw_traffic_id IS NOT NULL THEN 
			CAST(ct.original_cmw_traffic_id AS VARCHAR(MAX)) + '-R' + CAST(ct.version_number AS VARCHAR(MAX)) 
		ELSE 
			CAST(ct.id AS VARCHAR(MAX))
		END 'full_id',
		n.code 'network',
		s.code 'system',
		ctagy.name 'agency',
		ctadv.name 'advertiser',
		ISNULL(ctp.name, ctadv.name) 'product',
		ISNULL(e.firstname + ' ' + e.lastname,'') 'sales_person'
	FROM
		#filtered_cmw_traffic fct
		JOIN cmw_traffic ct						(NOLOCK) ON ISNULL(ct.original_cmw_traffic_id,ct.id)=fct.cmw_traffic_id AND ct.version_number=fct.max_version_number
		JOIN networks n							(NOLOCK) ON n.id=ct.network_id
		JOIN systems s							(NOLOCK) ON s.id=ct.system_id
		LEFT JOIN cmw_traffic_companies ctagy	(NOLOCK) ON ctagy.id  = ct.agency_cmw_traffic_company_id
		LEFT JOIN cmw_traffic_companies ctadv	(NOLOCK) ON ctadv.id  = ct.advertiser_cmw_traffic_company_id
		LEFT JOIN cmw_traffic_products ctp		(NOLOCK) ON ctp.id = ct.cmw_traffic_product_id
		LEFT JOIN cmw_traffic_details ctd		(NOLOCK) ON ctd.cmw_traffic_id=ct.id
		LEFT JOIN employees e					(NOLOCK) ON ct.salesperson_employee_id = e.id
	ORDER BY
		ct.id
		
	DROP TABLE #units_per_month;
	DROP TABLE #units;
	DROP TABLE #filtered_cmw_traffic;
END
