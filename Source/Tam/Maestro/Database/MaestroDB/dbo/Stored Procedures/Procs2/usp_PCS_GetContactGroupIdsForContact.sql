

-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].usp_PCS_GetContactGroupIdsForContact
	@contact_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT 
		contact_group_id 
	FROM 
		contact_group_contacts (NOLOCK)
	WHERE 
		contact_id=@contact_id
END


