-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 4/22/2011
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_ACS_GetMonthlyTrafficSystemMappings
	@media_month_id INT
AS
BEGIN
	SELECT DISTINCT
		mm.id,
		td.traffic_id,
		tr.system_id
	FROM 
		traffic_orders tr (NOLOCK) 
		JOIN traffic_details td (NOLOCK) ON td.id=tr.traffic_detail_id
		JOIN media_months mm (NOLOCK) ON (mm.start_date <= tr.end_date AND mm.end_date >= tr.start_date)
	WHERE
		mm.id=@media_month_id
END
