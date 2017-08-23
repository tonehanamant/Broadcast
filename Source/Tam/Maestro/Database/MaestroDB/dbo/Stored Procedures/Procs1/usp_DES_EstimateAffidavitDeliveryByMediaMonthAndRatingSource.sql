-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 9/20/2012
-- Modified:	06/27/2016 - Added ability to take in and filter on proposal Ids
--				10/16/2013 - added rating category group switch
-- Description:	Determines which delivery algorithm to use, pre april 2013 uses the original algorithm, 
--				april 2013 and on uses regional ratings algorithm.
-- =============================================
CREATE PROCEDURE [dbo].[usp_DES_EstimateAffidavitDeliveryByMediaMonthAndRatingSource]
	@idAffidavitDeliveryRun INT,
	@idMediaMonth INT,
	@idRatingSource INT,
	@stopHour INT,
	@proposalIds NVARCHAR(MAX)
AS
BEGIN
	DECLARE @year INT, @month INT, @rating_category_group_id TINYINT
	SELECT @year=mm.[year], @month=mm.[month] FROM dbo.media_months mm (NOLOCK) WHERE mm.id=@idMediaMonth
	SELECT @rating_category_group_id = dbo.GetRatingCategoryGroupIdOfRatingsSource(@idRatingSource)
	
	IF @rating_category_group_id = 3
	BEGIN
		EXEC dbo.usp_DES_EstimateRentrakAffidavitDeliveryByMediaMonthAndRatingSourceWithTimeZoneAdjustment 
				@idAffidavitDeliveryRun,
				@idMediaMonth,
				@idRatingSource,
				@stopHour
	END
	ELSE
	BEGIN
		IF @year>2013 OR (@year=2013 AND @month>=4)
			EXEC dbo.usp_DES_EstimateAffidavitDeliveryByMediaMonthAndRatingSourceWithTimeZoneAdjustment_v2 
				@idAffidavitDeliveryRun,
				@idMediaMonth,
				@idRatingSource,
				@stopHour,
				@proposalIds
		ELSE
			EXEC dbo.usp_DES_EstimateAffidavitDeliveryByMediaMonthAndRatingSourcePreApril2013 
				@idAffidavitDeliveryRun,
				@idMediaMonth,
				@idRatingSource
	END
END

/****** Object:  StoredProcedure [dbo].[usp_DES_EstimateAffidavitDeliveryByMediaMonthAndRatingSourceWithTimeZoneAdjustment_v2]    Script Date: 6/27/2016 10:33:36 AM ******/
SET ANSI_NULLS ON
