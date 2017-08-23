-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 6/2/2011
-- Description:	
-- =============================================
-- usp_PCS_ApplyExclusionsToPostedAffidavits 8
CREATE PROCEDURE [dbo].[usp_PCS_ApplyExclusionsToPostedAffidavits]
	@tam_post_id INT
AS
BEGIN
	DECLARE @id INT
	DECLARE @tam_post_proposal_id INT
	DECLARE @material_id INT
	DECLARE @network_id INT
	DECLARE @proposal_id INT
	
	CREATE TABLE #media_months (id INT)
	INSERT INTO #media_months
		SELECT DISTINCT
			p.posting_media_month_id
		FROM
			proposals p (NOLOCK)
			JOIN tam_post_proposals tpp (NOLOCK) ON tpp.posting_plan_proposal_id=p.id
				AND tpp.tam_post_id=@tam_post_id
	
	-- ENABLE all spots initially
	UPDATE 
		dbo.tam_post_affidavits
	SET 
		tam_post_affidavits.[enabled]=1 
	FROM
		dbo.tam_post_affidavits tpa
		JOIN dbo.tam_post_proposals tpp ON tpp.id=tpa.tam_post_proposal_id
			AND tpp.tam_post_id=@tam_post_id
	WHERE
		tpa.media_month_id IN (
			SELECT id FROM #media_months
		);
			
	
	CREATE TABLE #regional_sports_substitutions (network_id INT)
	INSERT INTO	#regional_sports_substitutions
		SELECT CAST(map_value AS INT) FROM dbo.network_maps (NOLOCK) WHERE map_set='PostReplace'
			

	DECLARE MyCursor CURSOR FOR 
		SELECT tpea.id,tpea.tam_post_proposal_id,tpea.material_id,tpea.network_id FROM tam_post_excluded_affidavits tpea (NOLOCK) WHERE tpea.tam_post_id=@tam_post_id

	OPEN MyCursor
		FETCH NEXT FROM MyCursor INTO @id,@tam_post_proposal_id,@material_id,@network_id

		WHILE @@FETCH_STATUS = 0
		BEGIN
			SELECT @proposal_id = tpp.posting_plan_proposal_id FROM tam_post_proposals tpp (NOLOCK) WHERE tpp.id=@tam_post_proposal_id

			CREATE TABLE #systems (system_id INT)
			IF (SELECT COUNT(*) FROM tam_post_excluded_affidavit_systems tpeas (NOLOCK) WHERE tpeas.tam_post_excluded_affidavit_id=@id) > 0
				BEGIN
					-- specific system(s) were chosen
					INSERT INTO #systems
						SELECT tpeas.system_id FROM tam_post_excluded_affidavit_systems tpeas (NOLOCK) WHERE tpeas.tam_post_excluded_affidavit_id=@id
				END
			ELSE
				BEGIN
					-- applies to all systems
					INSERT INTO #systems
						SELECT DISTINCT tpsd.system_id FROM tam_post_system_details tpsd (NOLOCK) JOIN tam_post_proposals tpp (NOLOCK) ON tpp.id=tpsd.tam_post_proposal_id AND tpp.tam_post_id=@tam_post_id
				END
			
			-- selectively DISABLE all records for this post matching criteria in [tam_post_excluded_affidavits]
			UPDATE
				dbo.tam_post_affidavits
			SET
				tam_post_affidavits.[enabled]=0
			FROM
				dbo.tam_post_affidavits tpa	(NOLOCK)
				JOIN #media_months mm				 ON mm.id=tpa.media_month_id
				JOIN tam_post_proposals tpp (NOLOCK) ON tpp.id=tpa.tam_post_proposal_id
					AND tpp.tam_post_id=@tam_post_id
				JOIN proposals p			(NOLOCK) ON p.id=tpp.posting_plan_proposal_id
				JOIN proposal_details pd	(NOLOCK) ON pd.id=tpa.proposal_detail_id
					AND (@proposal_id IS NULL OR pd.proposal_id=@proposal_id)
				JOIN affidavits a			(NOLOCK) ON a.media_month_id=p.posting_media_month_id
					AND a.id=tpa.affidavit_id
					AND (@material_id IS NULL OR a.material_id=@material_id)
					AND (@network_id  IS NULL OR ((@network_id=24 AND a.network_id IN (SELECT network_id FROM #regional_sports_substitutions)) OR (a.network_id=@network_id)))
				JOIN invoices i				(NOLOCK) ON i.id=a.invoice_id
					AND i.system_id IN (
						SELECT system_id FROM #systems
					)				
					
			DROP TABLE #systems;
				
				
			FETCH NEXT FROM MyCursor INTO @id,@tam_post_proposal_id,@material_id,@network_id
		END
	CLOSE MyCursor
	DEALLOCATE MyCursor
END
