-- =============================================
-- Author:		Nicholas Kheynis
-- Create date: <3/19/14>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_MSA_GetMediaMonthsForMSAFiles]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
		
	SELECT DISTINCT 
		mm.* 
	FROM 
		dbo.msa_posts mp
		JOIN media_months mm (NOLOCK) ON mm.id = mp.media_month_id
	ORDER BY 
		mm.start_date desc
END
