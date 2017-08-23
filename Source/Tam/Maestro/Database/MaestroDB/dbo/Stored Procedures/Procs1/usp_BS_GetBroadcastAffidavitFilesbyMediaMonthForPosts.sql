

/****** Object:  StoredProcedure [dbo].[usp_BS_GetBroadcastAffidavitFilesbyMediaMonthForPosts]    Script Date: 04/03/2014 08:35:31 ******/
-- =============================================
-- Author:		Nicholas Kheynis
-- Create date: <3/5/14>
-- Description:	<Description,,>
-- =============================================
-- usp_BS_GetBroadcastAffidavitFilesbyMediaMonthForPosts 385
CREATE PROCEDURE [dbo].[usp_BS_GetBroadcastAffidavitFilesbyMediaMonthForPosts]
	@MediaMonthId INT
AS
BEGIN
	SET NOCOUNT ON;
	
	SELECT
		baf.id,
		baf.file_name,
		baf.num_lines,
		baf.start_date,
		baf.end_date,
		bp.status_code
	FROM
		broadcast_affidavit_files baf (NOLOCK)
		JOIN media_months mm (NOLOCK) ON mm.start_date <= baf.end_date and mm.end_date >= baf.start_date
		LEFT JOIN dbo.broadcast_posts bp (NOLOCK) ON bp.broadcast_affidavit_file_id=baf.id
	WHERE 
		mm.id = @MediaMonthId
END
