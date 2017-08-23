-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 9/20/2012
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_MCS_GetMaterialsWithScreeners]
AS
BEGIN
	SELECT
		m.*
	FROM
		dbo.materials m (NOLOCK)
	WHERE
		m.has_screener=1
END
