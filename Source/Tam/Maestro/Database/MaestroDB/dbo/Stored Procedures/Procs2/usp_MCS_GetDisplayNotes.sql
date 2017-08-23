-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_MCS_GetDisplayNotes]
	@reference_id INT,
	@note_type VARCHAR(63)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT 
		notes.id,
		notes.employee_id,
		employees.firstname,
		employees.lastname,
		notes.comment,
		notes.date_created,
		notes.date_last_modified 
	FROM 
		notes (NOLOCK)
		LEFT JOIN employees (NOLOCK) ON employees.id=notes.employee_id 
	WHERE 
		reference_id=@reference_id
		AND note_type=@note_type 
	ORDER BY 
		date_created DESC
END

