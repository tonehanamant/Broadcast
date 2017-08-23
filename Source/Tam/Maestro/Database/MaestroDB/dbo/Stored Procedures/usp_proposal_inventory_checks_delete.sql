-- =============================================
-- Author:		CRUD Creator
-- Create date: 03/04/2016 03:27:10 PM
-- Description:	Auto-generated method to delete a single proposal_inventory_checks record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_proposal_inventory_checks_delete]
	@proposal_id INT,
	@date_created DATETIME
AS
BEGIN
	DELETE FROM
		[dbo].[proposal_inventory_checks]
	WHERE
		[proposal_id]=@proposal_id
		AND [date_created]=@date_created
END