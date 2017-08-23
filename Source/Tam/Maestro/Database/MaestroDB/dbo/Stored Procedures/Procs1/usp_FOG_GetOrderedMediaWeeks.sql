-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 9/20/2013
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_FOG_GetOrderedMediaWeeks]
	@topography_id INT,
	@media_month_id INT
AS
BEGIN
	SET NOCOUNT ON;
	
    SELECT
		mw.*
	FROM
		media_weeks mw (NOLOCK)
	WHERE
		mw.id IN (
			SELECT DISTINCT
				so.media_week_id
			FROM
				static_orders so (NOLOCK)
				JOIN media_weeks mw (NOLOCK) ON mw.id=so.media_week_id
					AND mw.media_month_id=@media_month_id
			WHERE
				so.topography_id=@topography_id
		)
	ORDER BY
		mw.start_date
END
