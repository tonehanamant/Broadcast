-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 4/1/2013
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PLS_GetCurrentMediaWeek]
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @current_date AS DATE
	SET @current_date = CONVERT (date, GETDATE())

    SELECT
		mw.*
	FROM
		dbo.media_weeks mw (NOLOCK)
	WHERE
		@current_date BETWEEN mw.start_date AND mw.end_date
END
