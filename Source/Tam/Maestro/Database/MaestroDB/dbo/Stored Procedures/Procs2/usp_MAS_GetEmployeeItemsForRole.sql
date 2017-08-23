-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_MAS_GetEmployeeItemsForRole] 
	@role_id INT
AS
BEGIN
	SET NOCOUNT ON;

    SELECT
		e.id,
		e.firstname,
		e.lastname
	FROM
		employees e (NOLOCK)
	WHERE
		e.id IN (
			SELECT employee_id FROM employee_roles (NOLOCK) WHERE role_id=@role_id
		)
	ORDER BY
		e.lastname
END
