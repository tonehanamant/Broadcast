-- =============================================
-- Author:		CRUD Creator
-- Create date: 08/25/2014 08:11:16 AM
-- Description:	Auto-generated method to insert a proposal_linkages record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_proposal_linkages_insert]
	@proposal_linkage_type TINYINT,
	@primary_proposal_id INT,
	@linked_proposal_id INT,
	@date_created DATETIME
AS
BEGIN
	INSERT INTO [dbo].[proposal_linkages]
	(
		[proposal_linkage_type],
		[primary_proposal_id],
		[linked_proposal_id],
		[date_created]
	)
	VALUES
	(
		@proposal_linkage_type,
		@primary_proposal_id,
		@linked_proposal_id,
		@date_created
	)
END
