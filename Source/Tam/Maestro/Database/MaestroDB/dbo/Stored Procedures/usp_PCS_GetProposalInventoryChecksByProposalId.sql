-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 3/1/2016
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetProposalInventoryChecksByProposalId]
	@proposal_id INT
AS
BEGIN
	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

    SELECT
		pic.*
	FROM
		dbo.proposal_inventory_checks pic
	WHERE
		pic.proposal_id=@proposal_id;
END