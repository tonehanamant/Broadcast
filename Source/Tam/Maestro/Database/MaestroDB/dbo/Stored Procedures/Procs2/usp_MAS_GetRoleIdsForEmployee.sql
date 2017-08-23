-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_MAS_GetRoleIdsForEmployee]
	@employee_id INT
AS
BEGIN
	SET NOCOUNT ON;

    SELECT 
		er.role_id 
	FROM 
		employee_roles er (NOLOCK)
	WHERE 
		er.employee_id=@employee_id
END
