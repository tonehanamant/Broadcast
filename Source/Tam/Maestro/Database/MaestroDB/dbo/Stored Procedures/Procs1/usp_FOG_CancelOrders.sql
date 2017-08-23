-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 9/20/2013
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_FOG_CancelOrders]
	@topography_id INT,
	@media_month_id INT,
	@media_week_ids VARCHAR(MAX),
	@system_ids VARCHAR(MAX),
	@proposal_ids VARCHAR(MAX)
AS
BEGIN
    DELETE FROM
		static_orders
	FROM
		static_orders so
		JOIN media_weeks mw (NOLOCK) ON mw.id=so.media_week_id
			AND mw.media_month_id=@media_month_id
	WHERE
		so.topography_id=@topography_id
		AND so.media_week_id IN (
			SELECT id FROM dbo.SplitIntegers(@media_week_ids)
		)
		AND so.system_id IN (
			SELECT id FROM dbo.SplitIntegers(@system_ids)
		)
		AND so.proposal_id IN (
			SELECT id FROM dbo.SplitIntegers(@proposal_ids)
		)
END
