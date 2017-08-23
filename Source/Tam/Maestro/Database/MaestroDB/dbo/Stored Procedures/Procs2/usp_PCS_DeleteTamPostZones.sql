-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 2/12/2013
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_DeleteTamPostZones]
	@tam_post_proposal_id INT
AS
BEGIN
	DELETE FROM tam_post_zones WHERE tam_post_proposal_id=@tam_post_proposal_id
END
