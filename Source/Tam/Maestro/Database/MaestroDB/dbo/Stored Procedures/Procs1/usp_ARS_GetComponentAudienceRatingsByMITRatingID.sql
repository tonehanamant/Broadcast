
-- =============================================
-- Author:		David Sisson
-- Create date: 5/20/09
-- Description:	Finds the set of actual ratings data, universe, TV usage and 
--				viewership, by rating category, component audience, and feed 
--				type linked to a mit_rating_id.
-- =============================================

CREATE PROCEDURE [dbo].[usp_ARS_GetComponentAudienceRatingsByMITRatingID] 
	@idMITRating as int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	BEGIN TRY
		-- Validate input parameters
		IF NOT EXISTS (SELECT * FROM mit_ratings WHERE id = @idMITRating)
			BEGIN
			RAISERROR(
				'Requested rating set, ID #%d, is not in the mit_ratings table.', 
				15, 
				1, 
				@idMITRating);
			END;

		-- Set and validate local variables
		-- Create output dataset
		select
			mr.rating_category_id,
			mpa.audience_id,
			mr.feed_type,
			mua.universe universe,
			mta.usage tv_usage,
			mpa.usage viewership
		from
			mit_ratings mr
			join mit_person_audiences mpa on
				mr.id = mpa.mit_rating_id
			join mit_tv_audiences mta on
				mr.id = mta.mit_rating_id
				and
				mpa.audience_id = mta.audience_id
			join mit_universes mu on
				mr.nielsen_network_id = mu.nielsen_network_id
				and
				mr.rating_date between mu.start_date and mu.end_date
				and
				mr.rating_category_id = mu.rating_category_id
			join mit_universe_audiences mua on
				mu.id = mua.mit_universe_id
				and
				mua.audience_id = mpa.audience_id
		where
			@idMITRating = mr.id
		order by
			dbo.GetAudienceSortPositionFromID(mpa.audience_id),
			mr.rating_category_id,
			mr.feed_type;
	END TRY
	BEGIN CATCH
		DECLARE @ErrorMessage NVARCHAR(4000);
		DECLARE @ErrorSeverity INT;
		DECLARE @ErrorState INT;

		SELECT 
			@ErrorMessage = ERROR_MESSAGE(),
			@ErrorSeverity = ERROR_SEVERITY(),
			@ErrorState = ERROR_STATE();

		-- Use RAISERROR inside the CATCH block to return error
		-- information about the original error that caused
		-- execution to jump to the CATCH block.
		RAISERROR (@ErrorMessage, -- Message text.
				   @ErrorSeverity, -- Severity.
				   @ErrorState -- State.
				   );
	END CATCH;
END
