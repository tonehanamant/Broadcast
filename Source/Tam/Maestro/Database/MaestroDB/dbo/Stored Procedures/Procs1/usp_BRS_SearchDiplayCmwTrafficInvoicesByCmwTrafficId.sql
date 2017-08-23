-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 9/12/2011
-- Description:	
-- =============================================
-- usp_BRS_SearchDiplayCmwTrafficInvoicesByCmwTrafficId 3043
CREATE PROCEDURE [dbo].[usp_BRS_SearchDiplayCmwTrafficInvoicesByCmwTrafficId]
	@cmw_traffic_id INT
AS
BEGIN
	DECLARE @max_version INT
	DECLARE @actual_cmw_traffic_id INT
	
	SELECT 
		@max_version = MAX(ct.version_number),
		@actual_cmw_traffic_id = ISNULL(ct.original_cmw_traffic_id,ct.id)
	FROM 
		cmw_traffic ct (NOLOCK) 
	WHERE 
		ct.id=@cmw_traffic_id OR ct.original_cmw_traffic_id=@cmw_traffic_id
	GROUP BY
		ct.original_cmw_traffic_id,
		ct.id
	
	CREATE TABLE #cmw_invoices (cmw_invoice_id INT)
	INSERT INTO #cmw_invoices
		SELECT DISTINCT
			cid.cmw_invoice_id
		FROM
			cmw_invoice_details cid (NOLOCK)
		WHERE
			cid.cmw_traffic_id=@actual_cmw_traffic_id
			
			
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
		cmw_invoices ci (NOLOCK)
		JOIN cmw_invoice_details cid (NOLOCK)	ON ci.id=cid.cmw_invoice_id AND cid.cmw_traffic_id=@actual_cmw_traffic_id
		JOIN media_months mm (NOLOCK)			ON mm.id=ci.media_month_id
		

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
		#cmw_invoices ci
		JOIN cmw_invoice_details cid (NOLOCK)			ON cid.cmw_invoice_id=ci.cmw_invoice_id
		JOIN cmw_traffic ct (NOLOCK)					ON ct.id=cid.cmw_traffic_id
		JOIN networks n (NOLOCK)						ON n.id=ct.network_id
		JOIN systems s (NOLOCK)							ON s.id=ct.system_id
		LEFT JOIN cmw_traffic_companies ctagy (NOLOCK)	ON ctagy.id  = ct.agency_cmw_traffic_company_id
		LEFT JOIN cmw_traffic_companies ctadv (NOLOCK)	ON ctadv.id  = ct.advertiser_cmw_traffic_company_id
		LEFT JOIN cmw_traffic_products ctp (NOLOCK)		ON ctp.id = ct.cmw_traffic_product_id
		LEFT JOIN employees e (NOLOCK)					ON ct.salesperson_employee_id = e.id
	ORDER BY
		ctadv.name
		
	DROP TABLE #cmw_invoices;
END
