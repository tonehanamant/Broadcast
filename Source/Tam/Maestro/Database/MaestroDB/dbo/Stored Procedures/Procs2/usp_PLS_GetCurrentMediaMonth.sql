-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 1/7/2013
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PLS_GetCurrentMediaMonth]
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @current_date DATETIME
	SET @current_date = CONVERT(VARCHAR, GETDATE(), 112);
	
    SELECT 
		mm.*
	FROM 
		dbo.media_months mm (NOLOCK)
	WHERE
		start_date<=@current_date AND end_date>=@current_date
END
