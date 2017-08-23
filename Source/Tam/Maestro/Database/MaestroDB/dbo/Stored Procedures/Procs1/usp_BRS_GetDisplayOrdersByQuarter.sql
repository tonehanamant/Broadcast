-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 9/15/2011
-- Description:	<Description,,>
-- =============================================
-- usp_BRS_GetDisplayOrdersByQuarter 2011,1
CREATE PROCEDURE usp_BRS_GetDisplayOrdersByQuarter
	@year INT,
	@quarter INT
AS
BEGIN
	CREATE TABLE #applicable_orders (id INT)
	INSERT INTO #applicable_orders
		SELECT DISTINCT
			ISNULL(ct.original_cmw_traffic_id,ct.id)
		FROM
			cmw_traffic_flights ctf (NOLOCK)
			JOIN media_months mm (NOLOCK) ON (mm.start_date <= ctf.end_date AND mm.end_date >= ctf.start_date)
			JOIN cmw_traffic ct (NOLOCK) ON ct.id=ctf.cmw_traffic_id
		WHERE
			mm.year = @year
			AND (@quarter IS NULL OR CASE mm.month WHEN 1 THEN 1 WHEN 2 THEN 1 WHEN 3 THEN 1 WHEN 4 THEN 2 WHEN 5 THEN 2 WHEN 6 THEN 2 WHEN 7 THEN 3 WHEN 8 THEN 3 WHEN 9 THEN 3 WHEN 10 THEN 4 WHEN 11 THEN 4 WHEN 12 THEN 4 END = @quarter)

	SELECT 
		ct.id,
		ct.sort_id,
		ct.display_id,
		ct.status_id,
		adv.[name] 'advertiser',
		pr.[name] 'product',
		agy.[name] 'agency',
		ct.release_name 'title',
		ct.flight_text,
		ct.date_created,
		ct.date_last_modified,
		ct.start_date,
		ct.end_date,
		s.code
	FROM 
		uvw_cmw_traffic ct (nolock)
		JOIN #applicable_orders ao ON ao.id=ct.sort_id
		LEFT JOIN cmw_traffic_companies adv (NOLOCK) ON adv.id=ct.advertiser_cmw_traffic_company_id 
		LEFT JOIN cmw_traffic_companies agy (NOLOCK) ON agy.id=ct.agency_cmw_traffic_company_id 
		LEFT JOIN cmw_traffic_products pr (NOLOCK) ON pr.id = ct.cmw_traffic_product_id
		LEFT JOIN systems s (NOLOCK) ON s.id = ct.system_id
	ORDER BY 
		ct.sort_id DESC
END
