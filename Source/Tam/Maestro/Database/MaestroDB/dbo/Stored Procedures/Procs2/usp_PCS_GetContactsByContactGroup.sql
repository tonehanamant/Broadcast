-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetContactsByContactGroup]
	@contact_group_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT 
		c.*
	FROM 
		contacts c (NOLOCK) 
	WHERE 
		c.id IN (
			SELECT contact_id FROM contact_group_contacts (NOLOCK) WHERE contact_group_id=@contact_group_id
		) 
	ORDER BY
		c.last_name
END
