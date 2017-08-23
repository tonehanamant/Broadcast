-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 5/19/2011
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_MCS_GetDisplayMaterials_ForMediaMonth]
	@media_month_id INT
AS
BEGIN
	DECLARE @start_date AS DATETIME
	DECLARE @end_date AS DATETIME
 
	SELECT @start_date=start_date, @end_date=end_date FROM media_months WHERE id=@media_month_id

    SELECT 
		dm.*
	FROM 
		uvw_display_materials dm 
	WHERE 
		(dm.date_created BETWEEN @start_date AND @end_date OR dm.date_received BETWEEN @start_date AND @end_date)
END
