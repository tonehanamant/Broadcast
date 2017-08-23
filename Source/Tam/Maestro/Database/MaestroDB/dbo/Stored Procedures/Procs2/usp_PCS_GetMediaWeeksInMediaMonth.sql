-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 11/4/2010
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetMediaWeeksInMediaMonth]
	@media_month_id INT
AS
BEGIN
	SELECT
		mw.*
	FROM
		media_weeks mw WITH(NOLOCK)
	WHERE
		mw.media_month_id=@media_month_id
	ORDER BY
		mw.start_date
END
