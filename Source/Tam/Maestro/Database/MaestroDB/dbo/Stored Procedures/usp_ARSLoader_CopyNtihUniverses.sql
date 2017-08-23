-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 6/4/2015
-- Description:	Copies TotUS NTIH Universes for every network received in NTIH Ratings for the flight specified.
-- =============================================
CREATE PROCEDURE [dbo].[usp_ARSLoader_CopyNtihUniverses]
	@start_date DATETIME,
	@end_date DATETIME
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @tot_us_nielsen_network_id INT;
	SET @tot_us_nielsen_network_id = 336;
	
	DECLARE @rating_category_id INT;
	SET @rating_category_id = 5;

	-- 1) get list of media month ids
    DECLARE @media_month_ids TABLE (
		media_month_id INT
	)
	INSERT INTO @media_month_ids
		SELECT
			mm.id
		FROM
			media_months mm (NOLOCK)
		WHERE
			mm.start_date <= @end_date AND mm.end_date >= @start_date;
			
	-- 2) get list of @mit_universe_ids to be copied
	DECLARE @mit_universe_ids TABLE(media_month_id INT, mit_universe_id INT);
	INSERT INTO @mit_universe_ids
		SELECT
			mu.media_month_id,
			mu.id 
		FROM 
			mit_universes mu (NOLOCK)
		WHERE
			mu.media_month_id IN (
				SELECT media_month_id FROM @media_month_ids
			)
			AND mu.rating_category_id = @rating_category_id
			AND mu.nielsen_network_id = @tot_us_nielsen_network_id;
	
	-- 3) get list of nielsen_network_ids received
	DECLARE @nielsen_network_ids TABLE (
		nielsen_network_id INT
	)
	INSERT INTO @nielsen_network_ids
		SELECT DISTINCT 
			mr.nielsen_network_id 
		FROM 
			mit_ratings mr (NOLOCK)   
		WHERE 
			mr.rating_category_id=@rating_category_id
			AND mr.media_month_id IN (
				SELECT media_month_id FROM @media_month_ids
			)
			AND (mr.rating_date <= @end_date AND mr.rating_date >= @start_date)
			AND mr.nielsen_network_id <> @tot_us_nielsen_network_id;
		
	-- 4) copy the TotUS universe data for every network we received in ratings during that flight
	DECLARE @mit_universe_id AS INT
	DECLARE @new_mit_universe_id AS INT
	DECLARE @nielsen_network_id AS INT
	DECLARE @media_month_id AS INT
	DECLARE MainCursor CURSOR FAST_FORWARD FOR
		SELECT
			media_month_id,
			mit_universe_id,
			nielsen_network_id 
		FROM 
			@mit_universe_ids
			CROSS APPLY @nielsen_network_ids;

	OPEN MainCursor
	FETCH NEXT FROM MainCursor INTO @media_month_id, @mit_universe_id, @nielsen_network_id
	WHILE @@FETCH_STATUS = 0
		BEGIN
			INSERT INTO mit_universes (media_month_id, rating_category_id, nielsen_network_id, start_date, end_date)
				SELECT 
					@media_month_id, @rating_category_id, @nielsen_network_id, start_date, end_date 
				FROM 
					mit_universes mu (NOLOCK) 
				WHERE 
					mu.rating_category_id=@rating_category_id 
					AND mu.media_month_id=@media_month_id 
					AND mu.id=@mit_universe_id;
				
			SELECT @new_mit_universe_id = SCOPE_IDENTITY();
			
			INSERT INTO mit_universe_audiences (media_month_id, mit_universe_id, audience_id, universe, effective_date)
				SELECT
					media_month_id, @new_mit_universe_id, audience_id, universe, effective_date
				FROM
					mit_universe_audiences mua (NOLOCK)
				WHERE
					mua.media_month_id=@media_month_id
					AND mua.mit_universe_id=@mit_universe_id;
				
				
			FETCH NEXT FROM MainCursor INTO @media_month_id, @mit_universe_id, @nielsen_network_id
		END
	CLOSE MainCursor
	DEALLOCATE MainCursor
END