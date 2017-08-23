-- =============================================
-- Author:		CRUD Creator
-- Create date: 06/26/2014 08:25:53 AM
-- Description:	Auto-generated method to insert a tam_post_analysis_reports_dayparts record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_tam_post_analysis_reports_dayparts_insert]
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
	INSERT INTO [dbo].[tam_post_analysis_reports_dayparts]
	(
		[tam_post_proposal_id],
		[audience_id],
		[media_week_id],
		[network_id],
		[daypart_id],
		[enabled],
		[subscribers],
		[delivery],
		[eq_delivery],
		[units],
		[dr_delivery],
		[dr_eq_delivery],
		[total_spots]
	)
	VALUES
	(
		@tam_post_proposal_id,
		@audience_id,
		@media_week_id,
		@network_id,
		@daypart_id,
		@enabled,
		@subscribers,
		@delivery,
		@eq_delivery,
		@units,
		@dr_delivery,
		@dr_eq_delivery,
		@total_spots
	)
END
