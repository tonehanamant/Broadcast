-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetOddNumberedMediaMonths]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT 
		id,
		media_month_id,
		week_number,
		start_date,
		end_date 
	FROM 
		media_weeks (NOLOCK) 
	WHERE 
		media_month_id IN (
			SELECT id FROM media_months (NOLOCK) WHERE month % 2 = 0
		)
END
