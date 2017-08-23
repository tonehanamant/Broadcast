-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetProposalMaterials]
	@proposal_id INT
AS
BEGIN
	SELECT
		pm.*
	FROM
		proposal_materials pm (NOLOCK) 
	WHERE
		pm.proposal_id=@proposal_id
END
