-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_RCS_GetDisplayNetworkRateCardBooksByMonth]
	@sales_model_id INT,
	@year INT,
	@media_month_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT
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
	WHERE
		nrcb.sales_model_id=@sales_model_id
		AND nrcb.year=@year
		AND nrcb.media_month_id=@media_month_id
END
