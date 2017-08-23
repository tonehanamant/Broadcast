-- =============================================
-- Author:		Mike Deaven
-- Create date: 8/29/2012
-- Description:	Calculate delivered ratings
-- =============================================
CREATE PROCEDURE [dbo].[usp_ARSLoader_DeliveredRatings]
	@date varchar(4)
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @lStartDate as datetime
	DECLARE @lEndDate as datetime
	DECLARE @media_month_id INT
	
	SELECT
		@media_month_id = MM.id,
		@lStartDate = MM.start_date,
		@lEndDate = MM.end_date 
	FROM 
		media_months MM (NOLOCK) 
	WHERE 
		MM.media_month = @date


	IF OBJECT_ID('tempdb..#TDR_Proposal_details') IS NOT NULL DROP TABLE #TDR_Proposal_details;

	CREATE TABLE #TDR_Proposal_details (Proposal_detail_id INT)

	INSERT INTO #TDR_Proposal_details
		SELECT DISTINCT
			PD.id [proposal_detail_id]
		FROM
			proposal_details PD (NOLOCK)
			JOIN proposals P (NOLOCK) on PD.proposal_id = P.id
			JOIN dayparts D (NOLOCK) on P.default_daypart_id = D.id
			JOIN timespans TS (NOLOCK) on D.timespan_id = TS.id
			JOIN products PR (NOLOCK) on P.product_id = PR.id
		WHERE
			TS.start_time < (3600 * 10)
			AND TS.end_time >= ((3600*24) - 100)
			AND P.proposal_status_id = 7
			AND P.start_date < @lEndDate
			AND P.end_date > @lStartDate
			AND PR.name not like '%Various%'

	DELETE
	FROM temp_db_backup.dbo.delivered_ratings_temp;

	INSERT INTO temp_db_backup.dbo.delivered_ratings_temp(
		period,
		abbr,
		daypart,
		impressions,
		subscribers,
		delivered_rating,
		year
		)
	SELECT
		@date [Period],
		temp1.Network,
		1,
		SUM(temp1.Impressions) [Imps],
		SUM(temp1.Subs) [Subs],
		100*(Imps/CAST(Subs AS FLOAT)),
		mm.year
	FROM
		(
			SELECT
				case
					when n.id in (select id from networks n (NOLOCK) where n.code in (select map_value from network_maps (NOLOCK) where network_id = 24 and map_set = 'traffic')) then 'FXSP'
					else n.code
				end as Network,
				CAST(A.subscribers as BIGINT) [Subs],
				CAST(A.subscribers as FLOAT) * (AD.audience_usage / AD.universe) [impressions]
			FROM
				invoices i								(NOLOCK)
				JOIN affidavits a						(NOLOCK)		on i.id = a.invoice_id
				JOIN systems s							(NOLOCK)		on i.system_id = s.id
				JOIN networks n							(NOLOCK)		on a.network_id = n.id
				JOIN zones Z							(NOLOCK)		on A.zone_id = Z.id
				JOIN zone_businesses ZB					(NOLOCK)		on Z.id = ZB.zone_id AND ZB.type = 'managedby'
				JOIN businesses B						(NOLOCK)		on ZB.business_id = B.id
				JOIN affidavit_deliveries AD			(NOLOCK)		on AD.media_month_id=@media_month_id AND A.id = AD.affidavit_id AND AD.audience_id = 31
				JOIN rating_source_rating_categories r	(NOLOCK)		ON AD.rating_source_id = r.rating_source_id AND r.rating_category_id = 1
				JOIN posted_affidavits PA				(NOLOCK)		on PA.media_month_id=@media_month_id AND A.id = PA.affidavit_id
				JOIN media_months mm 					(NOLOCK)		on A.media_Month_id = mm.id
			WHERE
				mm.media_month = @date 
				AND A.status_id in (1,3,7)
				AND B.name not in ('ECHO','DISH','GOOGLE','NETWORKS','DIRECTV','DIRECT')
				AND S.code not in ('ECHO','DISH','HITS','HOTG','SPIN','DIRECT','DIRECTV','CARVE')
				AND PA.proposal_detail_id in 
					(
						SELECT	TPD.proposal_detail_id
						FROM	#TDR_Proposal_details TPD
					)
		)temp1
	GROUP BY
		temp1.Network
		
	DROP TABLE #TDR_Proposal_details;
END
