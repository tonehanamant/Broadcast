-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetActiviesForContact]
	@contact_id INT
AS
BEGIN
	SELECT 
		a.*
	FROM 
		activities a (NOLOCK) 
	WHERE 
		a.contact_id=@contact_id 
	ORDER BY 
		a.date_created DESC
END
