-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 10/24/2013
-- Description:	Looks at the ratings table to see what has been forecasted and summarizes it.
--				This procedure is run periodically / after a forecast algorithm completes.
-- =============================================
CREATE PROCEDURE [dbo].[usp_ARS_RefreshForecastedMediaMonthsSummary]
AS
BEGIN
	SET NOCOUNT ON;
	
	CREATE TABLE #previously_forecasted (rating_category_id TINYINT, base_media_month_id INT, was_marked_forecasted BIT)
	INSERT INTO #previously_forecasted
		SELECT
			fmm.rating_category_id,
			fmm.base_media_month_id,
			fmm.is_forecasted
		FROM
			dbo.forecasted_media_months fmm (NOLOCK);

	CREATE TABLE #forecasted_ratings (rating_category_id TINYINT, base_media_month_id INT, forecast_media_month_id INT)
	INSERT INTO #forecasted_ratings
		SELECT
			r.rating_category_id,
			r.base_media_month_id,
			r.forecast_media_month_id
		FROM
			dbo.ratings r (NOLOCK)
		GROUP BY
			r.rating_category_id,
			r.base_media_month_id,
			r.forecast_media_month_id
			
	CREATE TABLE #forecasted_universes (rating_category_id TINYINT, base_media_month_id INT, total_forecasted_months INT)
	INSERT INTO #forecasted_universes
		SELECT
			u.rating_category_id,
			u.base_media_month_id,
			COUNT(DISTINCT u.forecast_media_month_id)
		FROM
			dbo.universes u (NOLOCK)
		GROUP BY
			u.rating_category_id,
			u.base_media_month_id;
		
	CREATE TABLE #expert_biases (media_month_id INT, total INT)
	INSERT INTO #expert_biases
		SELECT 
			eb.media_month_id,
			COUNT(1) 
		FROM 
			dbo.expert_biases eb (NOLOCK)
		GROUP BY
			eb.media_month_id;	
	
	CREATE TABLE #rotational_biases (media_month_id INT, total INT)
	INSERT INTO #rotational_biases
		SELECT 
			rbc.base_media_month_id,
			COUNT(1) 
		FROM 
			dbo.rotational_bias_coefficients rbc (NOLOCK)
		GROUP BY
			rbc.base_media_month_id;
	
	CREATE TABLE #base_months_prior_to (rating_category_id TINYINT, base_media_month_id INT, num_base_months_prior_to INT)
	INSERT INTO #base_months_prior_to
		SELECT
			fr.rating_category_id,
			fr.base_media_month_id,
			(SELECT COUNT(DISTINCT fr2.base_media_month_id) FROM #forecasted_ratings fr2 WHERE fr2.rating_category_id=fr.rating_category_id AND fr2.base_media_month_id<fr.base_media_month_id)
		FROM
			#forecasted_ratings fr (NOLOCK)
		GROUP BY
			fr.rating_category_id,
			fr.base_media_month_id
		ORDER BY
			fr.rating_category_id,
			fr.base_media_month_id;
	
	CREATE TABLE #forecasted_summary (base_media_month_id INT, rating_category_id TINYINT, start_forecast_media_month_id INT, end_forecast_media_month_id INT, is_forecasted BIT, is_expert_bias_loaded BIT, is_rotational_bias_loaded BIT, rating_category_group_id TINYINT)
	INSERT INTO #forecasted_summary
		SELECT
			f.base_media_month_id,
			f.rating_category_id,
			MIN(f.forecast_media_month_id),
			MAX(f.forecast_media_month_id),
			CASE 
				WHEN pf.was_marked_forecasted=1 THEN 1 
				WHEN COUNT(1) = 25 AND fu.total_forecasted_months = 25 AND bmp.num_base_months_prior_to>=23 THEN 1 
				ELSE 0 
			END 'is_forecasted',
			CASE WHEN pf.was_marked_forecasted=1 THEN 1 ELSE
				CASE WHEN eb.total IS NULL THEN 0 WHEN eb.total > 0 THEN 1 ELSE 0 END
			END 'is_expert_bias_loaded',
			CASE WHEN pf.was_marked_forecasted=1 THEN 1 ELSE
				CASE WHEN rb.total IS NULL THEN 0 WHEN rb.total > 0 THEN 1 ELSE 0 END
			END 'is_rotational_bias_loaded',
			rc.rating_category_group_id
		FROM
			#forecasted_ratings f
			JOIN rating_categories rc (NOLOCK) ON rc.id=f.rating_category_id
			LEFT JOIN #expert_biases eb ON eb.media_month_id=f.base_media_month_id
			LEFT JOIN #rotational_biases rb ON rb.media_month_id=f.base_media_month_id
			LEFT JOIN #forecasted_universes fu ON fu.rating_category_id=f.rating_category_id
				AND fu.base_media_month_id=f.base_media_month_id
			LEFT JOIN #base_months_prior_to bmp ON bmp.rating_category_id=f.rating_category_id
				AND bmp.base_media_month_id=f.base_media_month_id
			LEFT JOIN #previously_forecasted pf ON pf.rating_category_id=f.rating_category_id
				AND pf.base_media_month_id=f.base_media_month_id
		GROUP BY
			f.rating_category_id,
			f.base_media_month_id,
			fu.total_forecasted_months,
			eb.total,
			rb.total,
			bmp.num_base_months_prior_to,
			pf.was_marked_forecasted,
			rc.rating_category_group_id;
	
	UPDATE
		dbo.forecasted_media_months
	SET
		start_forecast_media_month_id=fs.start_forecast_media_month_id,
		end_forecast_media_month_id=fs.end_forecast_media_month_id,
		-- the case in here is saying if the rating category group is Rentrak then rotational bias does not need to be loaded
		is_forecasted=(fs.is_forecasted & fs.is_expert_bias_loaded & (fs.is_rotational_bias_loaded | CASE fs.rating_category_group_id WHEN 3 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END))
	FROM
		dbo.forecasted_media_months fmm
		JOIN #forecasted_summary fs ON fs.rating_category_id=fmm.rating_category_id
			AND fs.base_media_month_id=fmm.base_media_month_id
	
	INSERT INTO dbo.forecasted_media_months
		SELECT 
			fs.base_media_month_id, 
			fs.rating_category_id, 
			fs.start_forecast_media_month_id, 
			fs.end_forecast_media_month_id, 
			(fs.is_forecasted & fs.is_expert_bias_loaded & (fs.is_rotational_bias_loaded | CASE fs.rating_category_group_id WHEN 3 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END))
		FROM
			#forecasted_summary fs
			LEFT JOIN dbo.forecasted_media_months fmm ON fmm.rating_category_id=fs.rating_category_id
				AND fmm.base_media_month_id=fs.base_media_month_id
		WHERE
			fmm.base_media_month_id IS NULL
	
	DROP TABLE #forecasted_ratings;
	DROP TABLE #forecasted_summary;
	DROP TABLE #expert_biases;
	DROP TABLE #rotational_biases;
	DROP TABLE #base_months_prior_to;
	DROP TABLE #previously_forecasted;
END

