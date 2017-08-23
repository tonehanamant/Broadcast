-- =============================================
-- Author:        Nicholas Kheynis & Stephen Defusco
-- Create date:   8/19/2014
-- Description:   <Description,,>
-- =============================================
-- usp_ACCT_LookupForISCICode
CREATE PROCEDURE [dbo].[usp_ACCT_LookupForISCICode]
	@proposal_id INT
AS
BEGIN
	SELECT 
		  m.* 
	FROM
		  materials m (NOLOCK)
	WHERE
		m.id IN (
			SELECT
				  CASE WHEN rm.id IS NULL THEN m.id ELSE rm.id END 'material_id'
			FROM
				  proposal_materials pm (NOLOCK)
				  JOIN proposals p (NOLOCK) ON p.id=pm.proposal_id
				  JOIN materials m (NOLOCK) ON m.id=pm.material_id
				  JOIN proposals po (NOLOCK) ON po.id=p.original_proposal_id
				  LEFT JOIN materials rm (NOLOCK) ON rm.id=m.real_material_id
			WHERE
				  po.id=@proposal_id
	  )
END
