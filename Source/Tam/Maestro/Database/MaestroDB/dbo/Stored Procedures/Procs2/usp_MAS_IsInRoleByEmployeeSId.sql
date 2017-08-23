-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].usp_MAS_IsInRoleByEmployeeSId
	@role_id INT,
	@sid VARCHAR(63)
AS
BEGIN
	SELECT
		COUNT(*)
	FROM
		employee_roles (NOLOCK)
		JOIN employees (NOLOCK) ON employees.id=employee_roles.employee_id
	WHERE
		role_id=@role_id
		AND accountdomainsid=@sid
END
