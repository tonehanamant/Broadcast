-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].usp_MCS_GetPendingMaterialItemsByYear
	@year INT
AS
BEGIN
	SELECT 
		materials.id,
		materials.code,
		spot_lengths.length 
	FROM 
		materials (NOLOCK)
		JOIN spot_lengths (NOLOCK) ON spot_lengths.id=materials.spot_length_id 
	WHERE
		date_received IS NULL 
		AND year(date_created)=@year
	ORDER BY 
		code
END