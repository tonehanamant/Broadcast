
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[usp_MCS_GetTapeLogMaterials]
AS
BEGIN
	SELECT
		m.*
	FROM
		materials m (NOLOCK)
	WHERE
		m.tape_log=1
END


