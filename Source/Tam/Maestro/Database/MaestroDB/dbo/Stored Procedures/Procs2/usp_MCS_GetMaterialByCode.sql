-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 9/20/2012
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_MCS_GetMaterialByCode
	@code VARCHAR(31)
AS
BEGIN
	SELECT
		m.*
	FROM
		dbo.materials m (NOLOCK)
	WHERE
		m.code=@code
END
