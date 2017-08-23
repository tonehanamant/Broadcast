-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 07/20/2015
-- Modified:	08/31/2015 - updated to include optional @business_id parameter to perform business specific rotational bias.
-- Description:	Retreives ratings for a specific daypart_id.
-- =============================================
/*
DECLARE @hiatus_weeks FlightTable
INSERT INTO @hiatus_weeks SELECT '2015-10-12','2015-10-18'
SELECT * FROM dbo.udf_GetRatings(1,33,399,'2015-09-28','2015-11-29',1,7,1,@hiatus_weeks,NULL)
SELECT * FROM dbo.udf_GetRatings(1,33,399,'2015-09-28','2015-11-29',1,7,1,@hiatus_weeks,8) -- COMCAST SPECIFIC
*/
CREATE FUNCTION [dbo].[udf_GetRatings]
(	
	@network_id AS INT,
	@audience_id AS INT,
	@base_media_month_id AS INT,
	@start_date AS DATETIME,
	@end_date AS DATETIME,
	@daypart_id INT,
	@biases AS INT,
	@rating_source_id TINYINT,
	@hiatus_weeks FlightTable READONLY,
	@business_id AS INT -- OPTIONAL, if passed it will apply business specific rotational_bias (as long as that bias is enabled in @biases)
)
RETURNS @return TABLE 
(
	us_universe FLOAT,
	rating FLOAT
)
AS
BEGIN
	DECLARE 
		@start_time AS INT,
		@end_time AS INT,
		@mon AS BIT,
		@tue AS BIT,
		@wed AS BIT,
		@thu AS BIT,
		@fri AS BIT,
		@sat AS BIT,
		@sun AS BIT;
	
	-- get daypart details
	SELECT
		@start_time = d.start_time,
		@end_time = d.end_time,
		@mon = d.mon,
		@tue = d.tue,
		@wed = d.wed,
		@thu = d.thu,
		@fri = d.fri,
		@sat = d.sat,
		@sun = d.sun
	FROM
		dbo.vw_ccc_daypart d
	WHERE
		d.id=@daypart_id;
		
	INSERT INTO @return (us_universe,rating)
		SELECT us_universe,rating FROM dbo.udf_GetCustomRatings(
			@network_id,
			@audience_id,
			@base_media_month_id,
			@start_date,
			@end_date,
			@start_time,@end_time,@mon,@tue,@wed,@thu,@fri,@sat,@sun,
			@biases,
			@rating_source_id,
			@hiatus_weeks,
			@business_id);
	
	RETURN;
END