-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_PCS_GetChildDisplayContactGroups
	@parent_contact_group_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT 
		id,
		name 
	FROM 
		contact_groups (NOLOCK)
	WHERE 
		id IN (
			SELECT id FROM contact_groups (NOLOCK) WHERE parent_contact_group_id=@parent_contact_group_id
		)
END
