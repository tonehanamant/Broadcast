-- =============================================
-- Author:		CRUD Creator
-- Create date: 08/25/2014 08:11:17 AM
-- Description:	Auto-generated method to delete a single proposal_linkages record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_proposal_linkages_delete]
	@proposal_linkage_type TINYINT,
	@primary_proposal_id INT,
	@linked_proposal_id INT
AS
BEGIN
	DELETE FROM
		[dbo].[proposal_linkages]
	WHERE
		[proposal_linkage_type]=@proposal_linkage_type
		AND [primary_proposal_id]=@primary_proposal_id
		AND [linked_proposal_id]=@linked_proposal_id
END
