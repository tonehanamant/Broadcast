-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 7/21/2010
-- Edit date:	7/19/2012
--				5/14/2015 - Added output of adjusted_air_date and adjusted_air_time (aka est_air_datetime)
--				10/17/2016 - Removed hard coded daypart networks
--				01/24/2017 - replaced gmt calculation to read from field on affidavits table.
-- Description:	Retrieves the starting set of affidavits to post against the posting plan. 
--				The starting set is pre-filtered by the following:
--				1) The affidavit must be within network of the posting plan.
--				2) Copy in the posting plan (takes into account married spots and the flight of the copy as configured in the posting plan)
--				3) The affidavit must be valid (i.e. status_id=1)
--				4) The corresponding invoice of the affidavit must have a system_id.
--				5) The affidavit must have subscribers > 0
--				6) The air date must be between within flight of the posting plan and not between a hiatus week.
-- =============================================
-- usp_PCS_GetAffidavitDataToPost 50802,NULL
CREATE PROCEDURE [dbo].[usp_PCS_GetAffidavitDataToPost]
	@proposal_id INT,
	@previous_tam_post_id INT
AS
BEGIN
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED

	DECLARE @media_month_id INT
	SELECT @media_month_id = p.posting_media_month_id FROM proposals p WHERE p.id=@proposal_id

	DECLARE @media_month_start_date DATETIME;
	SELECT @media_month_start_date = mm.start_date FROM media_months mm WHERE mm.id=@media_month_id;

	CREATE TABLE #networks_in_plan (network_id INT)
	INSERT INTO #networks_in_plan
		SELECT DISTINCT network_id FROM proposal_details WHERE proposal_id=@proposal_id
				
	-- regional sports handling
	IF 24 IN (SELECT network_id FROM #networks_in_plan)
		INSERT INTO #networks_in_plan
			SELECT DISTINCT 
				CAST(nm.map_value AS INT) 
			FROM 
				dbo.uvw_networkmap_universe nm (NOLOCK) 
			WHERE 
				nm.map_set='PostReplace' 
				AND (nm.start_date<=@media_month_start_date AND (nm.end_date>=@media_month_start_date OR nm.end_date IS NULL)) 
				AND CAST(nm.map_value AS INT) NOT IN (SELECT network_id FROM #networks_in_plan)
	
	-- daypart network handling
	DECLARE @daypart_networks TABLE (network_id INT NOT NULL, substitute_network_id INT NOT NULL)
	INSERT INTO @daypart_networks
		SELECT 
			nm.network_id,
			CAST(nm.map_value AS INT) 
		FROM 
			dbo.uvw_networkmap_universe nm 
		WHERE 
			nm.map_set='DaypartNetworks' 
			AND flag IN (2,3)
			AND (nm.start_date<=@media_month_start_date AND (nm.end_date>=@media_month_start_date OR nm.end_date IS NULL));

	DECLARE @network_id INT;
	DECLARE @substitute_network_id INT;
	DECLARE @num_daypart_networks INT;
	SELECT @num_daypart_networks = COUNT(1) FROM @daypart_networks;

	WHILE @num_daypart_networks > 0
	BEGIN
		SELECT TOP 1
			@network_id = dn.network_id,
			@substitute_network_id = dn.substitute_network_id
		FROM
			@daypart_networks dn;

		IF @network_id IN (SELECT network_id FROM #networks_in_plan) AND @substitute_network_id NOT IN (SELECT network_id FROM #networks_in_plan)
			INSERT INTO #networks_in_plan SELECT @substitute_network_id

		DELETE FROM @daypart_networks WHERE network_id=@network_id AND substitute_network_id=@substitute_network_id;
		SELECT @num_daypart_networks = COUNT(1) FROM @daypart_networks;
	END


	CREATE TABLE #proposal_materials (proposal_id INT, material_id INT, start_date DATETIME, end_date DATETIME)
	INSERT INTO #proposal_materials
		SELECT pm.proposal_id, pm.material_id, pm.start_date, pm.end_date FROM proposal_materials pm JOIN materials m ON m.id=pm.material_id AND m.type<>'Married' WHERE pm.proposal_id=@proposal_id

	-- source_material_id in the case of married ISCI's is the component, if it's unmarried it's reflective.
	CREATE TABLE #tmp_materials (source_material_id INT, material_id INT, start_date DATETIME, end_date DATETIME)
	INSERT INTO #tmp_materials
		SELECT pm.material_id, pm.material_id, pm.start_date, pm.end_date FROM #proposal_materials pm WHERE pm.proposal_id=@proposal_id
	INSERT INTO #tmp_materials
		SELECT mr.revised_material_id, mr.original_material_id, pm.start_date, pm.end_date FROM #proposal_materials pm JOIN material_revisions mr ON mr.revised_material_id=pm.material_id WHERE pm.proposal_id=@proposal_id
		
	-- this is used for "Append To Post" mode (as in, we don't want to post anything that's already been posted)
	CREATE TABLE #previously_posted_affidavits (affidavit_id INT NOT NULL, material_id INT NOT NULL)
	IF @previous_tam_post_id IS NOT NULL
		BEGIN
			INSERT INTO #previously_posted_affidavits
				SELECT	
					tpa.affidavit_id, 
					tpa.posted_material_id 
				FROM 
					dbo.tam_post_affidavits tpa
					JOIN dbo.tam_post_proposals tpp ON tpp.id=tpa.tam_post_proposal_id
				WHERE 
					tpa.media_month_id=@media_month_id
					AND tpp.tam_post_id=@previous_tam_post_id
					AND tpp.posting_plan_proposal_id=@proposal_id
					AND tpp.post_source_code=0
				GROUP BY
					tpa.affidavit_id, 
					tpa.posted_material_id;
		END

	SELECT
		a.id,
		a.network_id,	
		a.air_date,
		a.air_time,
		m.source_material_id,
		a.material_id 'affidavit_material_id',
		i.system_id,
		a.zone_id,
		zb.business_id,
		a.subscribers,
		zd.dma_id,
		ISNULL(a.rate,0) 'rate',
		a.[program_name],
		DATEADD(SECOND,a.adjusted_air_time,CONVERT(DATETIME,a.adjusted_air_date)) 'est_air_datetime',
		a.gmt_air_datetime
	FROM
		dbo.affidavits a
		JOIN dbo.invoices i							ON i.id=a.invoice_id
		JOIN #tmp_materials	m						ON m.material_id=a.material_id AND a.air_date BETWEEN m.start_date AND m.end_date
		JOIN dbo.proposal_flights pf				ON pf.proposal_id=@proposal_id AND pf.selected=1 AND a.air_date BETWEEN pf.start_date AND pf.end_date
		LEFT JOIN #previously_posted_affidavits ppa	ON ppa.affidavit_id=a.id AND ppa.material_id=m.source_material_id
		JOIN #networks_in_plan nip					ON nip.network_id=a.network_id
		JOIN dbo.uvw_zonebusiness_universe zb		ON zb.zone_id=a.zone_id AND (zb.start_date<=a.air_date AND (zb.end_date>=a.air_date OR zb.end_date IS NULL)) AND zb.type='MANAGEDBY'
		JOIN dbo.uvw_zonedma_universe zd			ON zd.zone_id=a.zone_id AND (zd.start_date<=a.air_date AND (zd.end_date>=a.air_date OR zd.end_date IS NULL)) 
	WHERE
		a.media_month_id=@media_month_id
		AND a.status_id=1
		AND a.subscribers<>0
		AND ppa.affidavit_id IS NULL
		AND i.system_id IS NOT NULL

	DROP TABLE #tmp_materials;
	DROP TABLE #proposal_materials;
	DROP TABLE #previously_posted_affidavits;	
	DROP TABLE #networks_in_plan;
END
