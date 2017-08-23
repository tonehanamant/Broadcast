-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetYears]
AS
BEGIN
	SELECT
		DISTINCT mm.[year]
	FROM
		media_months mm (NOLOCK)
	ORDER BY
		mm.[year] DESC
END
