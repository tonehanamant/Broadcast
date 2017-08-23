-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_MAS_GetRolesForEmployee]
	@employee_id INT
AS
BEGIN
	SELECT
		r.*
	FROM
		roles r (NOLOCK)
	WHERE
		r.id IN (
			SELECT
				role_id
			FROM
				employee_roles (NOLOCK)
			WHERE
				employee_id=@employee_id
		)
END
