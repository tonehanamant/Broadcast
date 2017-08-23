
CREATE PROCEDURE [dbo].[usp_PCS_GetDisplayNotesForContact]
	@contact_id INT,
	@note_type VARCHAR(63)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT 
		notes.id,
		notes.employee_id,
		notes.comment,
		notes.date_created,
		notes.date_last_modified 
	FROM 
		notes (NOLOCK) 
	WHERE 
		reference_id=@contact_id 
		AND note_type=@note_type
	ORDER BY 
		date_created DESC
END
