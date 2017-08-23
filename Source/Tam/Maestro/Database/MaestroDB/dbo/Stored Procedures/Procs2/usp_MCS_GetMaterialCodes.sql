
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 7/21/2011
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[usp_MCS_GetMaterialCodes]
AS
BEGIN
	SELECT
		m.code,
		m.id
	FROM
		materials m (NOLOCK)
END

