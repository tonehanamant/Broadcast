-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_ARS_GetNielsenSuggestedRates]
	@year INT,
	@quarter INT,
	@media_month_id INT
AS
BEGIN
	SELECT
		id,
		year,
		quarter,
		media_month_id
	FROM
		nielsen_suggested_rates
	WHERE
		year=@year
		AND quarter=@quarter
		AND ((@media_month_id IS NULL AND media_month_id IS NULL) OR media_month_id=@media_month_id)
END