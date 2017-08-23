-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 2/12/2013
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetTamPostMaterials]
	@tam_post_proposal_id INT
AS
BEGIN
	SELECT tpm.* FROM tam_post_materials tpm (NOLOCK) WHERE tpm.tam_post_proposal_id=@tam_post_proposal_id
END
