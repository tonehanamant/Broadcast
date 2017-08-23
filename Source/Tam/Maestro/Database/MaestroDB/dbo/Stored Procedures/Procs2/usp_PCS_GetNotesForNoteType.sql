
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].usp_PCS_GetNotesForNoteType
	@note_type VARCHAR(15),
	@reference_id INT
AS
BEGIN
	SELECT 
		id,
		note_type,
		reference_id,
		employee_id,
		comment,
		date_created,
		date_last_modified 
	FROM 
		notes (NOLOCK)
	WHERE 
		reference_id=@reference_id 
		AND note_type=@note_type 
	ORDER BY 
		date_created DESC
END
