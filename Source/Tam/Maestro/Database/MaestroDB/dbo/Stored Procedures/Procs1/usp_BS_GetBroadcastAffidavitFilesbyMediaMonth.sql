-- =============================================
-- Author:		Nicholas Kheynis
-- Create date: <2/12/14>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_BS_GetBroadcastAffidavitFilesbyMediaMonth]
	@MediaMonthId INT
AS
BEGIN
	SET NOCOUNT ON;
	
	SELECT 
		baf.id,
		baf.file_size,
		baf.file_name,
		baf.num_lines,
		baf.start_date,
		baf.end_date
	FROM 
		broadcast_affidavit_files baf (NOLOCK)
		JOIN media_months mm (NOLOCK) ON mm.start_date <= baf.end_date and mm.end_date >= baf.start_date
	WHERE 
		mm.id = @MediaMonthId
END
