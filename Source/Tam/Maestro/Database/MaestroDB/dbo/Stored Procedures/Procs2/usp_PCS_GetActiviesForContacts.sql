-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetActiviesForContacts]
	@contact_ids VARCHAR(MAX)
AS
BEGIN
	SELECT 
		a.*
	FROM 
		activities a (NOLOCK) 
	WHERE 
		a.contact_id IN (
			SELECT id FROM dbo.SplitIntegers(@contact_ids)
		)
	ORDER BY 
		a.date_created DESC
END
