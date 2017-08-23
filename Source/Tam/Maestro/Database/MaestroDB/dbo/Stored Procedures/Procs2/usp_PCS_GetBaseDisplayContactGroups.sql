-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetBaseDisplayContactGroups]
AS
BEGIN
	SELECT 
		id,
		name 
	FROM 
		contact_groups (NOLOCK) 
	WHERE 
		parent_contact_group_id IS NULL
END
