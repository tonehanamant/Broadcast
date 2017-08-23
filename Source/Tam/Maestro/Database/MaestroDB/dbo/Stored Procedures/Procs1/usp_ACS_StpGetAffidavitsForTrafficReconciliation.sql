-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 2/25/2011
-- Description:	
-- =============================================
-- usp_ACS_StpGetAffidavitsForTrafficReconciliation 392
CREATE PROCEDURE [dbo].[usp_ACS_StpGetAffidavitsForTrafficReconciliation]
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
		affidavits a (NOLOCK)
		JOIN invoices i (NOLOCK) ON i.id=a.invoice_id
	WHERE 
		a.media_month_id=@media_month_id
		AND a.status_id=1
		AND i.system_id IS NOT NULL
		AND a.id NOT IN (
			--SELECT DISTINCT
			--	pa.affidavit_id
			--FROM
			--	posted_affidavits pa (NOLOCK)
			--WHERE
			--	pa.media_month_id=@media_month_id
				
			--UNION
			
			SELECT DISTINCT
				at.affidavit_id
			FROM
				affidavit_traffic at (NOLOCK)
			WHERE
				at.media_month_id=@media_month_id
				AND at.status_code=1
		)
END
