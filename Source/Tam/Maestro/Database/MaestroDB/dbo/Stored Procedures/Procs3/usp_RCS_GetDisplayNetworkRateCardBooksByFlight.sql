-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 11/6/2012
-- Description:	<Description,,>
-- =============================================
-- usp_RCS_GetDisplayNetworkRateCardBooksByFlight 1,'10/1/2012','12/30/2012'
CREATE PROCEDURE [dbo].[usp_RCS_GetDisplayNetworkRateCardBooksByFlight]
	@sales_model_id INT,
	@start_date DATETIME,
	@end_date DATETIME
AS
BEGIN
	SET NOCOUNT ON;

    SELECT DISTINCT
		nrcb.*,
		mm1.media_month,
		e.firstname + ' ' + e.lastname [employee],
		mm2.media_month [base_coverage_universe_media_month],
		mm3.media_month [base_ratings_media_month]
	FROM
		network_rate_card_books nrcb	(NOLOCK)
		LEFT JOIN media_months mm1		(NOLOCK) ON mm1.id=nrcb.media_month_id
		LEFT JOIN employees e			(NOLOCK) ON e.id=nrcb.approved_by_employee_id
		JOIN media_months mm2			(NOLOCK) ON mm2.id=nrcb.base_coverage_universe_media_month_id
		JOIN media_months mm3			(NOLOCK) ON mm3.id=nrcb.base_ratings_media_month_id
		JOIN media_months mm			(NOLOCK) ON mm.[year]=nrcb.[year] AND CASE mm.[month] WHEN 1 THEN 1 WHEN 2 THEN 1 WHEN 3 THEN 1 WHEN 4 THEN 2 WHEN 5 THEN 2 WHEN 6 THEN 2 WHEN 7 THEN 3 WHEN 8 THEN 3 WHEN 9 THEN 3 WHEN 10 THEN 4 WHEN 11 THEN 4 WHEN 12 THEN 4 END=nrcb.[quarter]
			AND (mm.start_date <= @end_date AND mm.end_date >= @start_date)
	WHERE
		nrcb.sales_model_id=@sales_model_id
		AND nrcb.date_approved IS NOT NULL
	ORDER BY
		nrcb.[year] DESC,
		nrcb.[quarter] DESC,
		nrcb.[version] DESC
END
