-- =============================================
-- Author:		Nicholas Kheynis
-- Create date: <2/12/14>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_BS_GetMediaMonthsForAffidavitFiles]

AS
BEGIN
	SET NOCOUNT ON;
		
	SELECT DISTINCT 
		mm.* 
	FROM 
		broadcast_affidavit_files baf (NOLOCK)
		JOIN media_months mm (NOLOCK) ON mm.start_date <= baf.end_date and mm.end_date >= baf.start_date
	ORDER BY 
		mm.start_date desc
END
