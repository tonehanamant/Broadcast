-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_BRS_GetEmployeesByRoleID]
	@Role_id int
AS
BEGIN
	SET NOCOUNT ON;
	
	SELECT
		e.*
	FROM
		employees e (NOLOCK)
		JOIN employee_roles er (NOLOCK) ON er.employee_id = e.id
	WHERE
		er.role_id = @Role_id
	ORDER BY
		e.lastname ASC
END

