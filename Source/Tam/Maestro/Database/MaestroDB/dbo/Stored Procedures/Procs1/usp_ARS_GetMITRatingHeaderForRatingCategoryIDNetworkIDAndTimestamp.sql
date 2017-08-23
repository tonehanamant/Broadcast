-- =============================================
-- Author:		David Sisson
-- Create date: 5/20/09
-- Description:	Finds the set of actual ratings data, universe, TV usage and 
--				viewership, by rating category, component audience, and feed 
--				type for a network, on a date and at a time.
-- =============================================

CREATE PROCEDURE [dbo].[usp_ARS_GetMITRatingHeaderForRatingCategoryIDNetworkIDAndTimestamp] 
	@dateTimestamp as datetime,
	@idNetwork as int,
	@idRatingCategory as int,
	@idMITRating as int output,
	@dateStartTimestamp as datetime output,
	@dateEndTimestamp as datetime output,
	@szFeedType as varchar(15) output
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	declare
		@idNielsenNetwork as int,
		@codeNielsenID as int,
		@stringDate as varchar(63),
		@stringMapSet as varchar(15),
		@media_month_id INT;

	BEGIN TRY
		-- Validate input parameters
		IF NOT EXISTS (SELECT * FROM networks WHERE id = @idNetwork)
			BEGIN
			RAISERROR(
				'Requested network, ID #%d, is not in the networks table.', 
				15, 
				1, 
				@idNetwork);
			END;

		-- Set and validate local variables
		set @stringDate = convert(varchar,@dateTimestamp,111);
		set @stringMapSet = 'Nielsen';
		
		SELECT @media_month_id = mm.id FROM media_months mm (NOLOCK) WHERE @dateTimestamp BETWEEN mm.start_date AND mm.end_date;
	
		with
			nielsen_network_map(
				nielsen_id
			) as (
				select
					cast(map_value as int) nielsen_id
				from
					network_maps
				where
					1 = active
					and
					@stringMapSet = map_set
					and
					@idNetwork = network_id
					and
					@dateTimestamp >= effective_date
				union
				select
					cast(map_value as int) nielsen_id
				from
					network_map_histories
				where
					1 = active
					and
					@stringMapSet = map_set
					and
					@idNetwork = network_id
					and
					@dateTimestamp between start_date and end_date
			)
		select
			@codeNielsenID = nielsen_id
		from
			nielsen_network_map;
		IF @codeNielsenID IS NULL
			BEGIN
			RAISERROR(
				'Requested network id, %d, does not map to a Nielsen ID on %s.', 
				15, 
				1, 
				@idNetwork,
				@stringDate);
			END;

		with
			nielsen_network(
				nielsen_network_id
			) as (
				select
					id 
				from
					nielsen_networks nn
				where
					@codeNielsenID = nielsen_id
					and
					@dateTimestamp >= effective_date
				union
				select
					nielsen_network_id 
				from
					nielsen_network_histories nn
				where
					@codeNielsenID = nielsen_id
					and
					@dateTimestamp between start_date and end_date
			)
		select	
			@idNielsenNetwork = nielsen_network_id
		from
			nielsen_network;
		IF @idNielsenNetwork IS NULL
			BEGIN
			RAISERROR(
				'Requested network, ID %d and Nielsen ID %d, does not have a corresponding Nielsen Network on %s.', 
				15, 
				1, 
				@idNetwork,
				@codeNielsenID,
				@stringDate);
			END;

		-- Create output dataset
		select
			@idMITRating = mr.id, 
			@szFeedType = mr.feed_type, 
			@dateStartTimestamp = dateadd(second, mr.start_time, mr.rating_date), 
			@dateEndTimestamp = dateadd(second, mr.end_time, mr.rating_date)
		from
			mit_ratings mr (NOLOCK)
		where
			@media_month_id = mr.media_month_id
			and
			@idRatingCategory = mr.rating_category_id
			and
			@idNielsenNetwork = mr.nielsen_network_id
			and
			@dateTimestamp between 
				dateadd(second, mr.start_time, mr.rating_date) 
				and 
				dateadd(second, mr.end_time, mr.rating_date);
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
