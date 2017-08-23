-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 9/8/2015
-- Changes:		03/01/2016 - Stephen DeFusco - Modified to look back "n" months to address the issue where posts for a month aren't run until 2 media months later.
--				08/19/2016 - Stpehen DeFusco - Modified to support new table name "inventory_inputs".
-- Description:	Given the current media month these procedure figures out which month(s) need to be calculated.
-- =============================================
CREATE PROCEDURE [dbo].[usp_ICS_CanCrunchInventoryInputs]
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @current_media_month_id INT;
	SELECT @current_media_month_id = mm.id FROM dbo.media_months mm (NOLOCK) WHERE CONVERT(date, GETDATE()) BETWEEN mm.start_date AND mm.end_date;
	
	DECLARE @media_month_id_one_month_prior INT;
	SELECT @media_month_id_one_month_prior = dbo.udf_CalculateFutureMediaMonthId(@current_media_month_id, -1);

	CREATE TABLE #potential_media_months (media_month_id INT NOT NULL);
	INSERT INTO #potential_media_months
		SELECT mm.id FROM dbo.media_months mm WHERE mm.id<=@media_month_id_one_month_prior AND mm.id>399
		EXCEPT
		SELECT ii.media_month_id FROM inventory.dbo.inventory_inputs ii (NOLOCK) WHERE ii.media_month_id>399 GROUP BY ii.media_month_id;

	SELECT 
		pmm.media_month_id, 
		CASE WHEN s.perc_posts_posted >= 99 AND s.perc_posts_aggregated >= 99 THEN
			CAST(1 AS BIT) -- if 99% of the posts in this month have been posted and aggregated we can safely use the results of the posts in maestro_analysis
		ELSE
			CAST(0 AS BIT) 
		END 'ready_to_calculate'
	FROM
		#potential_media_months pmm
		CROSS APPLY dbo.udf_GetAffidavitPostingSummary(pmm.media_month_id,0) s
	ORDER BY
		pmm.media_month_id;

	DROP TABLE #potential_media_months;
END