-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetContactGroupsByIds]
	@ids VARCHAR(MAX)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT 
		id,
		parent_contact_group_id,
		name,
		description 
	FROM 
		contact_groups (NOLOCK) 
	WHERE 
		id IN (
			SELECT id FROM dbo.SplitIntegers(@ids)
		)
END
