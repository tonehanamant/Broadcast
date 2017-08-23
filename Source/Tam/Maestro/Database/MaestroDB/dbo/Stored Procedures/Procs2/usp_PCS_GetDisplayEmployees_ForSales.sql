-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetDisplayEmployees_ForSales]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT 
		id,
		firstname,
		lastname,
		email,
		phone 
	FROM 
		employees (NOLOCK) 
	WHERE 
		id IN (
			SELECT employee_id FROM employee_roles (NOLOCK) WHERE role_id=7
		) 
	ORDER BY 
		lastname
END
