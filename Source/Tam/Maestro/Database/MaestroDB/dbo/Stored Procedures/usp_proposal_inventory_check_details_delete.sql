-- =============================================
-- Author:		CRUD Creator
-- Create date: 02/26/2016 11:22:45 AM
-- Description:	Auto-generated method to delete a single proposal_inventory_check_details record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_proposal_inventory_check_details_delete]
	@proposal_id INT,
	@proposal_detail_id INT,
	@date_created DATETIME
AS
BEGIN
	DELETE FROM
		[dbo].[proposal_inventory_check_details]
	WHERE
		[proposal_id]=@proposal_id
		AND [proposal_detail_id]=@proposal_detail_id
		AND [date_created]=@date_created
END