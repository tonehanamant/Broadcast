
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].usp_PCS_GetNotesForCompanies
	@note_type VARCHAR(15),
	@ids VARCHAR(MAX)
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
		note_type=@note_type 
		AND reference_id IN (
			SELECT id FROM dbo.SplitIntegers(@ids)
		)
END