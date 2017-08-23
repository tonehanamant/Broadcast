-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 11/5/2014
-- Description:	
-- =============================================
CREATE PROCEDURE usp_TCS_GetInternalTrafficNote
	@traffic_id INT
AS
BEGIN
	SELECT 
		n.* 
	FROM
		notes n (NOLOCK) 
		JOIN traffic t (NOLOCK) ON n.id=t.internal_note_id 
	WHERE 
		t.id=@traffic_id
END
