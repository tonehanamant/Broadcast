-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 11/5/2014
-- Description:	
-- =============================================
CREATE PROCEDURE usp_TCS_GetExternalTrafficNote
	@traffic_id INT
AS
BEGIN
	SELECT 
		n.id,
		n.reference_id,
		n.comment,
		n.date_created,
		n.date_last_modified,
		e.username
	FROM
		notes n (NOLOCK) 
		JOIN traffic t (NOLOCK) ON n.id=t.external_note_id 
		JOIN employees e (NOLOCK) ON e.id=n.employee_id
	WHERE 
		t.id=@traffic_id
END
