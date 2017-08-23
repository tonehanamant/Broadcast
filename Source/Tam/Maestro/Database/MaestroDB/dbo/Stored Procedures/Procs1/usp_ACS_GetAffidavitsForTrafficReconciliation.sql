-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 2/25/2011
-- Description:	
-- =============================================
-- usp_ACS_GetAffidavitsForTrafficReconciliation 356
CREATE PROCEDURE [dbo].[usp_ACS_GetAffidavitsForTrafficReconciliation]
	@media_month_id INT
AS
BEGIN
	SELECT 
		a.id,
		i.system_id,
		a.network_id,
		a.material_id,
		a.air_date,
		a.air_time,
		a.traffic_id,
		a.zone_id,
		a.subscribers,
		CAST(a.rate/100.0 AS MONEY)
	FROM 
		affidavits a WITH(NOLOCK) 
		JOIN invoices i WITH(NOLOCK) ON i.id=a.invoice_id
		LEFT JOIN affidavit_traffic at WITH(NOLOCK) ON at.media_month_id=@media_month_id
			AND at.affidavit_id=a.id
	WHERE 
		a.media_month_id=@media_month_id 
		AND a.status_id=1 
		AND a.traffic_id IS NOT NULL 
		AND i.system_id IS NOT NULL
		AND at.affidavit_id IS NULL
END
