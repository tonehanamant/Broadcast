-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 7/21/2011
-- Description:	Retrieves all HD material items.
-- =============================================
-- usp_MCS_GetMaterialItems_HdByYear 2011
CREATE PROCEDURE [dbo].[usp_MCS_GetMaterialItems_HdByYear]
	@year INT
AS
BEGIN
    SELECT 
		m.id,
		m.code,
		sl.length 
	FROM 
		materials m (NOLOCK)
		JOIN spot_lengths sl (NOLOCK) ON sl.id=m.spot_length_id
	WHERE
		m.is_hd=1
		AND (year(m.date_received)=@year OR year(m.date_created)=@year)
	ORDER BY 
		m.code
END
