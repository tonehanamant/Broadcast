-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 11/18/2011
-- Description:	
-- =============================================
-- usp_PCS_GetSbtsNotDelivering 1001819,NULL
CREATE PROCEDURE [dbo].[usp_PCS_GetSbtsNotDelivering]
	@tam_post_id INT,
	@proposal_ids VARCHAR(MAX)
AS
BEGIN
	DECLARE @effective_date DATETIME
	CREATE TABLE #proposal_ids (proposal_id INT)
	IF @proposal_ids IS NULL
		BEGIN
			INSERT INTO #proposal_ids
				SELECT tpp.posting_plan_proposal_id FROM tam_post_proposals tpp (NOLOCK) WHERE tpp.tam_post_id=@tam_post_id
		END
	ELSE
		BEGIN
			INSERT INTO #proposal_ids
				SELECT id FROM dbo.SplitIntegers(@proposal_ids)
		END
		
	DECLARE @sales_model_id INT
	SELECT @sales_model_id = MIN(psm.sales_model_id) FROM proposal_sales_models psm (NOLOCK) WHERE psm.proposal_id IN (SELECT proposal_id FROM #proposal_ids)
	CREATE TABLE #system_ids_to_ignore (system_id INT)
	IF @sales_model_id = 1
	BEGIN
		INSERT INTO #system_ids_to_ignore
			SELECT DISTINCT
				tpsd.system_id
			FROM
				tam_post_system_details tpsd (NOLOCK)
				JOIN tam_post_proposals tpp (NOLOCK) ON tpp.id=tpsd.tam_post_proposal_id
				JOIN proposal_sales_models psm (NOLOCK) ON psm.proposal_id=tpp.posting_plan_proposal_id
					AND psm.sales_model_id IN (2,3)
				
			EXCEPT

			SELECT DISTINCT
				tpsd.system_id
			FROM
				tam_post_system_details tpsd (NOLOCK)
				JOIN tam_post_proposals tpp (NOLOCK) ON tpp.id=tpsd.tam_post_proposal_id
				JOIN proposal_sales_models psm (NOLOCK) ON psm.proposal_id=tpp.posting_plan_proposal_id
					AND psm.sales_model_id IN (1)
	END
	
	-- determine most appropriate effective date to pull system codes with
	SELECT
		@effective_date = MIN(p.start_date)
	FROM
		tam_post_proposals tpp (NOLOCK)
		JOIN proposals p (NOLOCK) ON p.id=tpp.posting_plan_proposal_id
			AND p.id IN (SELECT proposal_id FROM #proposal_ids)
	WHERE
		tpp.tam_post_id=@tam_post_id
		
	-- calculate list of media to use when looking at systems we got invoices for		
	CREATE TABLE #media_months (media_month_id INT)
	INSERT INTO #media_months
		SELECT
			mm.id
		FROM
			media_months mm (NOLOCK)
		WHERE
			(mm.start_date <= @effective_date AND mm.end_date >= DATEADD(d,-180,@effective_date))
		
	-- return active systems who didn't deliver anything towards this post but did delivery to other posts in this timeframe
	SELECT DISTINCT
		b.business_id,
		b.name 'mso',
		s.system_id,
		s.code 'system',
		s.location
	FROM
		uvw_system_universe s
		JOIN uvw_systemzone_universe sz ON sz.system_id=s.system_id
			AND sz.type='BILLING'
			AND sz.start_date<=@effective_date AND (sz.end_date>=@effective_date OR sz.end_date IS NULL)
		JOIN uvw_zonebusiness_universe zb (NOLOCK) ON zb.zone_id=sz.zone_id 
			AND zb.type='MANAGEDBY' 
			AND zb.start_date<=@effective_date AND (zb.end_date>=@effective_date OR zb.end_date IS NULL)
		JOIN uvw_business_universe b ON b.business_id=zb.business_id
			AND b.start_date<=@effective_date AND (b.end_date>=@effective_date OR b.end_date IS NULL)
	WHERE
		s.system_id NOT IN (
			SELECT DISTINCT
				tpsd.system_id 
			FROM 
				tam_post_system_details tpsd (NOLOCK)
				JOIN tam_post_proposals tpp (NOLOCK) ON tpp.id=tpsd.tam_post_proposal_id
			WHERE
				tpp.tam_post_id=@tam_post_id
				AND tpp.posting_plan_proposal_id IN (
					SELECT proposal_id FROM #proposal_ids
				)
				
			UNION ALL
			
			SELECT system_id FROM #system_ids_to_ignore
		)
		AND s.system_id IN (
			SELECT DISTINCT 
				system_id
			FROM
				invoices i (NOLOCK)
			WHERE
				i.system_id IS NOT NULL
				AND i.media_month_id IN (
					SELECT media_month_id FROM #media_months
				)
		)
		AND s.active=1
		AND s.start_date<=@effective_date AND (s.end_date>=@effective_date OR s.end_date IS NULL)
	ORDER BY
		s.code
	
	DROP TABLE #proposal_ids;
	DROP TABLE #media_months;
	DROP TABLE #system_ids_to_ignore;
END
