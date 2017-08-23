-- =============================================
-- Author:		Nicholas Kheynis
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_BS_GetDisplayBroadcastAffidavitFile]
	@id INT
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
	WHERE
		baf.id=@id
END
