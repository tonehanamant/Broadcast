-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].usp_MCS_GetMaterialItemsByIds
	@ids VARCHAR(MAX)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT 
		materials.id,
		materials.code,
		spot_lengths.length 
	FROM 
		materials (NOLOCK)
		JOIN spot_lengths (NOLOCK) ON spot_lengths.id=materials.spot_length_id
	WHERE
		materials.id IN (
			SELECT id FROM dbo.SplitIntegers(@ids)
		)
	ORDER BY 
		code
END