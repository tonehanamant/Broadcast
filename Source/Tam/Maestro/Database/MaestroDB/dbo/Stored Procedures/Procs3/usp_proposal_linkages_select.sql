-- =============================================
-- Author:		CRUD Creator
-- Create date: 08/25/2014 08:11:16 AM
-- Description:	Auto-generated method to delete or potentionally disable a proposal_linkages record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_proposal_linkages_select]
	@proposal_linkage_type TINYINT,
	@primary_proposal_id INT,
	@linked_proposal_id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		[proposal_linkages].*
	FROM
		[dbo].[proposal_linkages] WITH(NOLOCK)
	WHERE
		[proposal_linkage_type]=@proposal_linkage_type
		AND [primary_proposal_id]=@primary_proposal_id
		AND [linked_proposal_id]=@linked_proposal_id
END
