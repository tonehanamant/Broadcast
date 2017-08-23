-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_MAS_GetEmployeeRolesByEmployeeId]
	@employee_id INT
AS
BEGIN
	SELECT 
		er.*
	FROM 
		employee_roles er (NOLOCK)
	WHERE 
		er.employee_id=@employee_id
END
