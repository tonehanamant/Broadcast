
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_RCS_IsCoverageUniverseBaseMonthValid]
	@sales_model_id INT,
	@base_media_month_id INT
AS
BEGIN
	SELECT
		COUNT(*)
	FROM
		coverage_universes (NOLOCK)
	WHERE
		sales_model_id=@sales_model_id
		AND base_media_month_id=@base_media_month_id
		AND date_approved IS NOT NULL
END

