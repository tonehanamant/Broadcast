-- =============================================
-- Author:		CRUD Creator
-- Create date: 08/25/2014 08:11:16 AM
-- Description:	Auto-generated method to update a proposal_linkages record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_proposal_linkages_update]
	@proposal_linkage_type TINYINT,
	@primary_proposal_id INT,
	@linked_proposal_id INT,
	@date_created DATETIME
AS
BEGIN
	UPDATE
		[dbo].[proposal_linkages]
	SET
		[date_created]=@date_created
	WHERE
		[proposal_linkage_type]=@proposal_linkage_type
		AND [primary_proposal_id]=@primary_proposal_id
		AND [linked_proposal_id]=@linked_proposal_id
END
