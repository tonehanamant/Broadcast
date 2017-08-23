
-- =============================================
-- Author:		David Sisson
-- Create date: 5/20/09
-- Description:	Finds the set of actual ratings data, universe, TV usage and 
--				viewership, by component audience, rating category, and feed 
--				type for a network, on a date and at a time.
-- =============================================

CREATE PROCEDURE [dbo].[usp_ARS_GetComponentAudienceRatingsByNetworkAndTimestamp] 
	@idNetwork int,
	@timestamp datetime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	declare
		@idNielsenNetwork as int,
		@codeNielsenID as int,
		@date as datetime,
		@time as int,
		@stringDate as varchar(63),
		@stringMapSet as varchar(15);

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
		set @stringDate = convert(varchar, @timestamp, 120);
		set @time = (datepart(hour, @timestamp) * 60 * 60) + (datepart(minute, @timestamp) * 60) + datepart(second, @timestamp);
		set @date = convert(varchar, @timestamp, 111);
		IF @time not between 0 and 86399 
			BEGIN
			RAISERROR(
				'Internal error in stored procedure, usp_ARS_GetComponentAudienceRatingsByNetworkAndTimestamp: Number of seconds after midnight, %d, is not being calculted correctly for timestamp, %s. Result must be between 0 and 86399.', 
				15, 
				1, 
				@time,
				@stringDate);
			END;
		EXECUTE usp_ARS_GetComponentAudienceRatingsByNetworkDateAndTime
			@idNetwork,
			@date,
			@time;
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
