-- =============================================
-- Author:		CRUD Creator
-- Create date: 03/04/2016 03:27:10 PM
-- Description:	Auto-generated method to update a proposal_inventory_checks record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_proposal_inventory_checks_update]
	@proposal_id INT,
	@date_created DATETIME,
	@proposal_status_id INT,
	@base_media_month_id INT,
	@health_score FLOAT,
	@total_allocated_subscribers BIGINT,
	@total_contracted_subscribers BIGINT,
	@total_forecasted_subscribers BIGINT,
	@ims_duration INT
AS
BEGIN
	UPDATE
		[dbo].[proposal_inventory_checks]
	SET
		[proposal_status_id]=@proposal_status_id,
		[base_media_month_id]=@base_media_month_id,
		[health_score]=@health_score,
		[total_allocated_subscribers]=@total_allocated_subscribers,
		[total_contracted_subscribers]=@total_contracted_subscribers,
		[total_forecasted_subscribers]=@total_forecasted_subscribers,
		[ims_duration]=@ims_duration
	WHERE
		[proposal_id]=@proposal_id
		AND [date_created]=@date_created
END