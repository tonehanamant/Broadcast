-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 12/13/2013
-- Description:	Used in delivery calculation to map networks to nielsen networks and apply substitutions.
-- =============================================
-- usp_PST_GetMonthlyPostingNetworkToNielsenNetworkAndSubstitutionMap 380,1
CREATE PROCEDURE [dbo].[usp_PST_GetMonthlyPostingNetworkToNielsenNetworkAndSubstitutionMap]
	@media_month_id INT,
	@rating_source_id TINYINT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @start_date DATETIME
	DECLARE @end_date DATETIME
	DECLARE @iterator_date DATETIME
	DECLARE @rating_category_group_id TINYINT

	SELECT 
		@start_date=mm.start_date, 
		@end_date=mm.end_date 
	FROM 
		media_months mm (NOLOCK) 
	WHERE 
		mm.id=@media_month_id
		
	SELECT @rating_category_group_id = dbo.GetRatingCategoryGroupIdOfRatingsSource(@rating_source_id)

	CREATE TABLE #output (date_of DATETIME NOT NULL, network_id INT NOT NULL, nielsen_network_id INT NULL, rating_weight FLOAT NOT NULL, universe_weight FLOAT NOT NULL, rating_network_id INT NULL, universe_network_id INT NULL, rating_nielsen_network_id INT NULL, universe_nielsen_network_id INT NULL)
	-- this should always hold true, if is does not, something is wrong with network_maps or nielsen_networks data such that effective dates are overlapping
	CREATE UNIQUE CLUSTERED INDEX IX_output_pk ON #output(date_of ASC, network_id ASC)

	-- table of rate networks in the month in question
	CREATE TABLE #rated_networks (network_id INT, nielsen_network_id INT);		
	INSERT INTO #rated_networks (network_id, nielsen_network_id)
		SELECT
			nm.network_id,
			mr.nielsen_network_id
		FROM
			mit_ratings mr
			JOIN uvw_nielsen_network_universes nn ON nn.nielsen_network_id=mr.nielsen_network_id
				AND (nn.start_date<=mr.rating_date AND (nn.end_date>=mr.rating_date OR nn.end_date IS NULL))
				AND nn.active=1
			JOIN uvw_networkmap_universe nm ON nm.map_set='Nielsen' 
				AND (nm.start_date<=mr.rating_date AND (nm.end_date>=mr.rating_date OR nm.end_date IS NULL))
				AND CAST(nm.map_value AS INT)=nn.nielsen_id
				AND nm.active=1
			JOIN rating_source_rating_categories rsrc ON rsrc.rating_source_id=@rating_source_id
				AND rsrc.rating_category_id=mr.rating_category_id
		WHERE
			mr.media_month_id=@media_month_id
		GROUP BY
			nm.network_id,
			mr.nielsen_network_id
			
		INTERSECT
					
		SELECT
			nm.network_id,
			mu.nielsen_network_id
		FROM
			mit_universes mu
			JOIN uvw_nielsen_network_universes nn ON nn.nielsen_network_id=mu.nielsen_network_id
				AND (nn.start_date<=mu.start_date AND (nn.end_date>=mu.start_date OR nn.end_date IS NULL))
				AND nn.active=1
			JOIN uvw_networkmap_universe nm ON nm.map_set='Nielsen' 
				AND (nm.start_date<=mu.start_date AND (nm.end_date>=mu.start_date OR nm.end_date IS NULL))
				AND CAST(nm.map_value AS INT)=nn.nielsen_id
				AND nm.active=1
			JOIN rating_source_rating_categories rsrc ON rsrc.rating_source_id=@rating_source_id
				AND rsrc.rating_category_id=mu.rating_category_id
		WHERE
			mu.media_month_id=@media_month_id
		GROUP BY
			nm.network_id,
			mu.nielsen_network_id

	-- table of the days in the month
	CREATE TABLE #days_in_month (date_of DATE)
	SET @iterator_date = @start_date
	WHILE @iterator_date <= @end_date
	BEGIN
		INSERT INTO #days_in_month (date_of) VALUES (@iterator_date)	
		SET @iterator_date = DATEADD(d,1,@iterator_date)
	END

	-- 1) all nielsen network mappings
	INSERT INTO #output
		SELECT
			dim.date_of,
			nm.network_id,
			nn.nielsen_network_id,
			1.0,
			1.0,
			nm.network_id,
			nm.network_id,
			nn.nielsen_network_id,
			nn.nielsen_network_id
		FROM
			#days_in_month dim
			LEFT JOIN uvw_networkmap_universe nm ON nm.map_set='Nielsen' 
				AND (nm.start_date<=dim.date_of AND (nm.end_date>=dim.date_of OR nm.end_date IS NULL))
				AND nm.active=1
			LEFT JOIN uvw_nielsen_network_universes nn ON nn.nielsen_id=CAST(nm.map_value AS INT)
				AND (nn.start_date<=dim.date_of AND (nn.end_date>=dim.date_of OR nn.end_date IS NULL))
				AND nn.active=1
							
	-- 2) remove regional sports nets (if they're in there which some will be from the query above)
	DELETE FROM #output WHERE network_id IN (
		SELECT 
			CAST(nm.map_value AS INT) 
		FROM 
			network_maps nm (NOLOCK) 
		WHERE 
			nm.map_set='PostReplace' 
			AND CAST(nm.map_value AS INT)<>nm.network_id
	)

	-- 3) apply network substitutions (delivery) for unrated networks
	UPDATE
		#output
	SET
		rating_weight=ns.weight,
		rating_network_id=ns.substitute_network_id,
		rating_nielsen_network_id=nn.nielsen_network_id
	FROM
		#output o
		LEFT JOIN #rated_networks rn ON rn.network_id=o.network_id
		JOIN uvw_network_substitutions ns ON ns.network_id=o.network_id
			AND (ns.start_date<=o.date_of AND (ns.end_date>=o.date_of OR ns.end_date IS NULL)) 
			AND ns.rating_category_group_id=@rating_category_group_id
			AND ns.substitution_category_id=1
		JOIN uvw_networkmap_universe nm ON nm.map_set='Nielsen' 
			AND nm.network_id=ns.substitute_network_id
			AND (nm.start_date<=o.date_of AND (nm.end_date>=o.date_of OR nm.end_date IS NULL))
			AND nm.active=1
		JOIN uvw_nielsen_network_universes nn ON nn.nielsen_id=CAST(nm.map_value AS INT)
			AND (nn.start_date<=o.date_of AND (nn.end_date>=o.date_of OR nn.end_date IS NULL)) 
			AND nn.active=1
	WHERE
		rn.network_id IS NULL -- we only want to substitute non-rated networks
		
		
	-- 3) apply network substitutions (universe) for unrated networks
	UPDATE
		#output
	SET
		universe_weight=ns.weight,
		universe_network_id=ns.substitute_network_id,
		universe_nielsen_network_id=nn.nielsen_network_id
	FROM
		#output o
		LEFT JOIN #rated_networks rn ON rn.network_id=o.network_id
		JOIN uvw_network_substitutions ns ON ns.network_id=o.network_id
			AND (ns.start_date<=o.date_of AND (ns.end_date>=o.date_of OR ns.end_date IS NULL)) 
			AND ns.rating_category_group_id=@rating_category_group_id
			AND ns.substitution_category_id=2
		JOIN uvw_networkmap_universe nm ON nm.map_set='Nielsen' 
			AND nm.network_id=ns.substitute_network_id
			AND (nm.start_date<=o.date_of AND (nm.end_date>=o.date_of OR nm.end_date IS NULL))
			AND nm.active=1
		JOIN uvw_nielsen_network_universes nn ON nn.nielsen_id=CAST(nm.map_value AS INT)
			AND (nn.start_date<=o.date_of AND (nn.end_date>=o.date_of OR nn.end_date IS NULL)) 
			AND nn.active=1
	WHERE
		rn.network_id IS NULL -- we only want to substitute non-rated networks
		
	-- copy regional sports net nielsen network mappings
	INSERT INTO #output
		SELECT
			o.date_of,
			CAST(nm.map_value AS INT) 'network_id',
			NULL,
			o.rating_weight,
			o.universe_weight,
			o.rating_network_id,
			o.universe_network_id,
			o.rating_nielsen_network_id,
			o.universe_nielsen_network_id
		FROM
			#output o
			JOIN uvw_networkmap_universe nm ON nm.map_set='PostReplace'
				AND (nm.start_date<=o.date_of AND (nm.end_date>=o.date_of OR nm.end_date IS NULL))
				AND nm.network_id=o.network_id
				AND nm.active=1
		ORDER BY
			nm.map_value
	
	-- output results
	SELECT
		*
	FROM
		#output
	WHERE
		rating_nielsen_network_id IS NOT NULL
		AND universe_nielsen_network_id IS NOT NULL
	ORDER BY
		date_of,
		network_id

	DROP TABLE #days_in_month;
	DROP TABLE #output;
	DROP TABLE #rated_networks;
END
