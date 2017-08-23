-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 12/26/2012
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_ARS_GetAudienceMaps
	@map_set VARCHAR(63)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		am.*
	FROM
		dbo.audience_maps am (NOLOCK)
	WHERE
		am.map_set=@map_set
END
