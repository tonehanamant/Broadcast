-- =============================================
-- Author:		CRUD Creator
-- Create date: 06/26/2014 08:25:53 AM
-- Description:	Auto-generated method to update a tam_post_analysis_reports_dayparts record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_tam_post_analysis_reports_dayparts_update]
	@tam_post_proposal_id INT,
	@audience_id INT,
	@media_week_id INT,
	@network_id INT,
	@daypart_id INT,
	@enabled BIT,
	@subscribers BIGINT,
	@delivery FLOAT,
	@eq_delivery FLOAT,
	@units FLOAT,
	@dr_delivery FLOAT,
	@dr_eq_delivery FLOAT,
	@total_spots INT
AS
BEGIN
	UPDATE
		[dbo].[tam_post_analysis_reports_dayparts]
	SET
		[subscribers]=@subscribers,
		[delivery]=@delivery,
		[eq_delivery]=@eq_delivery,
		[units]=@units,
		[dr_delivery]=@dr_delivery,
		[dr_eq_delivery]=@dr_eq_delivery,
		[total_spots]=@total_spots
	WHERE
		[tam_post_proposal_id]=@tam_post_proposal_id
		AND [audience_id]=@audience_id
		AND [media_week_id]=@media_week_id
		AND [network_id]=@network_id
		AND [daypart_id]=@daypart_id
		AND [enabled]=@enabled
END
