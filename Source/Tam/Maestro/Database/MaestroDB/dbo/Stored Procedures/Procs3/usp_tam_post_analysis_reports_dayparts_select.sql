-- =============================================
-- Author:		CRUD Creator
-- Create date: 06/26/2014 08:25:53 AM
-- Description:	Auto-generated method to delete or potentionally disable a tam_post_analysis_reports_dayparts record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_tam_post_analysis_reports_dayparts_select]
	@tam_post_proposal_id INT,
	@audience_id INT,
	@media_week_id INT,
	@network_id INT,
	@daypart_id INT,
	@enabled BIT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[tam_post_analysis_reports_dayparts].*
	FROM
		[dbo].[tam_post_analysis_reports_dayparts] WITH(NOLOCK)
	WHERE
		[tam_post_proposal_id]=@tam_post_proposal_id
		AND [audience_id]=@audience_id
		AND [media_week_id]=@media_week_id
		AND [network_id]=@network_id
		AND [daypart_id]=@daypart_id
		AND [enabled]=@enabled
END
