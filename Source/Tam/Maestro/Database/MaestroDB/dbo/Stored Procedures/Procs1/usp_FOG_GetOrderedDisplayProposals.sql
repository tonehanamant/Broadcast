-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 9/20/2013
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_FOG_GetOrderedDisplayProposals]
	@topography_id INT,
	@media_month_id INT
AS
BEGIN
	SET NOCOUNT ON;
	
    SELECT
		dp.*
	FROM
		uvw_display_proposals dp (NOLOCK)
	WHERE
		dp.id IN (
			SELECT DISTINCT
				so.proposal_id
			FROM
				static_orders so (NOLOCK)
				JOIN media_weeks mw (NOLOCK) ON mw.id=so.media_week_id
					AND mw.media_month_id=@media_month_id
			WHERE
				so.topography_id=@topography_id
		)
	ORDER BY
		dp.product
END
