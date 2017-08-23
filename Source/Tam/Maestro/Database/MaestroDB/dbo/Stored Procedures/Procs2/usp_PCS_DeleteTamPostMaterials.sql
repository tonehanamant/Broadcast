-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 7/21/2012
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_PCS_DeleteTamPostMaterials
	@tam_post_proposal_id INT
AS
BEGIN
	DELETE FROM tam_post_materials WHERE tam_post_proposal_id=@tam_post_proposal_id
END
