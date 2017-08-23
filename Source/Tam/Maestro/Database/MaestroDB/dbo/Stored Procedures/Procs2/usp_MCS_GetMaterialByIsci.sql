-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_MCS_GetMaterialByIsci]
	@isci VARCHAR(31)
AS
BEGIN
    SELECT
		*
	FROM
		materials (NOLOCK)
	WHERE	
		code=LTRIM(RTRIM(@isci))
END
