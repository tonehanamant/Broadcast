-- =============================================
-- Author:			Stephen DeFusco
-- Create date:		A long time ago
-- Modified date:	10/15/2013
-- Description:		<Description,,>
-- =============================================
/*
DECLARE @hiatus_weeks FlightTable
INSERT INTO @hiatus_weeks SELECT '2015-10-12','2015-10-18'
EXEC dbo.usp_ARS_GetCustomRatings 1,33,399,'2015-09-28','2015-11-29',72000,86399,1,1,1,1,1,1,1,7,1,@hiatus_weeks,NULL
*/
CREATE PROCEDURE [dbo].[usp_ARS_GetCustomRatings]
	@network_id AS INT,
	@audience_id AS INT,
	@base_media_month_id AS INT,
	@start_date AS DATETIME,
	@end_date AS DATETIME,
	@start_time AS INT,
	@end_time AS INT,
	@mon AS BIT,
	@tue AS BIT,
	@wed AS BIT,
	@thu AS BIT,
	@fri AS BIT,
	@sat AS BIT,
	@sun AS BIT,
	@biases AS INT,
	@rating_source_id TINYINT,
	@hiatus_weeks FlightTable READONLY,
	@business_id INT -- OPTIONAL, used for MSO specific rotational bias, only used when @biases is turned on for @bias_rotational
AS
BEGIN
	SELECT * FROM dbo.udf_GetCustomRatings(@network_id,@audience_id,@base_media_month_id,@start_date,@end_date,@start_time,@end_time,@mon,@tue,@wed,@thu,@fri,@sat,@sun,@biases,@rating_source_id,@hiatus_weeks,@business_id);
END