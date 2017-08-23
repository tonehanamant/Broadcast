﻿-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 9/12/2011
-- Description:	
-- =============================================
-- usp_BRS_GetDiplayCmwTrafficInvoicesByQuarter 2011,1
CREATE PROCEDURE [dbo].[usp_BRS_GetDiplayCmwTrafficInvoicesByQuarter]
	@year INT,
	@quarter INT
AS
BEGIN
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
				cmw_invoices ci (NOLOCK)
				JOIN media_months mm (NOLOCK) ON mm.id=ci.media_month_id
					AND mm.year = @year
					AND (@quarter IS NULL OR CASE mm.month WHEN 1 THEN 1 WHEN 2 THEN 1 WHEN 3 THEN 1 WHEN 4 THEN 2 WHEN 5 THEN 2 WHEN 6 THEN 2 WHEN 7 THEN 3 WHEN 8 THEN 3 WHEN 9 THEN 3 WHEN 10 THEN 4 WHEN 11 THEN 4 WHEN 12 THEN 4 END = @quarter)
				JOIN cmw_invoice_details cid (NOLOCK) ON cid.cmw_invoice_id=ci.id
				JOIN cmw_traffic ct (NOLOCK) ON cid.cmw_traffic_id=ISNULL(ct.original_cmw_traffic_id,ct.id)
		) ct
		GROUP BY
			ct.id
			
	-- invoice data
	SELECT DISTINCT
		ci.id,
		ci.media_month_id,
		mm.media_month,
		ci.gross_due,
		ci.agency_commission_fee,
		ci.net_due_to_rep,
		ci.rep_commission_fee,
		ci.net_due_to_network
	FROM
		#filtered_cmw_traffic fct
		JOIN cmw_traffic ct (NOLOCK) ON ISNULL(ct.original_cmw_traffic_id,ct.id)=fct.cmw_traffic_id AND ct.version_number=fct.max_version_number
		JOIN cmw_invoice_details cid (NOLOCK) ON cid.cmw_traffic_id=ISNULL(ct.original_cmw_traffic_id,ct.id)
		JOIN cmw_invoices ci (NOLOCK) ON ci.id=cid.cmw_invoice_id
		JOIN media_months mm (NOLOCK) ON mm.id=ci.media_month_id
	ORDER BY
		ci.id
		
	-- corresponding cmw_traffic information (could be more than one)
	SELECT
		cid.cmw_invoice_id,
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
		ctadv.id 'advertiser_company_id',
		ISNULL(ctp.name, ctadv.name) 'product',
		ISNULL(e.firstname + ' ' + e.lastname,'') 'sales_person'
	FROM
		#filtered_cmw_traffic fct
		JOIN cmw_traffic ct (NOLOCK)					ON ISNULL(ct.original_cmw_traffic_id,ct.id)=fct.cmw_traffic_id AND ct.version_number=fct.max_version_number
		JOIN cmw_invoice_details cid (NOLOCK)			ON cid.cmw_traffic_id=ISNULL(ct.original_cmw_traffic_id,ct.id)
		JOIN networks n (NOLOCK)						ON n.id=ct.network_id
		JOIN systems s (NOLOCK)							ON s.id=ct.system_id
		LEFT JOIN cmw_traffic_companies ctagy (NOLOCK)	ON ctagy.id  = ct.agency_cmw_traffic_company_id
		LEFT JOIN cmw_traffic_companies ctadv (NOLOCK)	ON ctadv.id  = ct.advertiser_cmw_traffic_company_id
		LEFT JOIN cmw_traffic_products ctp (NOLOCK)		ON ctp.id = ct.cmw_traffic_product_id
		LEFT JOIN employees e (NOLOCK)					ON ct.salesperson_employee_id = e.id
	ORDER BY
		cid.cmw_invoice_id
		
	DROP TABLE #filtered_cmw_traffic;
END
