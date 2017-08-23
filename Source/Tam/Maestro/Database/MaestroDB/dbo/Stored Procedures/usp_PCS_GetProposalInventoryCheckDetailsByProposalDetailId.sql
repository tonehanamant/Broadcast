-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 3/1/2016
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetProposalInventoryCheckDetailsByProposalDetailId]
	@proposal_id INT,
	@proposal_detail_id INT
AS
BEGIN
	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

    SELECT
		picd.*
	FROM
		dbo.proposal_inventory_check_details picd
	WHERE
		picd.proposal_id=@proposal_id
		AND picd.proposal_detail_id=@proposal_detail_id;
END