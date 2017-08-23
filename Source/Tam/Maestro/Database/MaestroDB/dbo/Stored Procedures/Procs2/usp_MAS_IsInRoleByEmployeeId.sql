-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_MAS_IsInRoleByEmployeeId]
	@employee_id INT,
	@role_id INT
AS
BEGIN
	SELECT
		COUNT(*)
	FROM
		employee_roles (NOLOCK)
	WHERE
		role_id=@role_id
		AND employee_id=@employee_id
END
