-- =============================================
-- Author:		John Carsley
-- Create date: 10/13/2011
-- Description:	Returns the names and ids of active employees.  Usually used to fill GenericValue instances in Maestro Admin
-- =============================================
CREATE PROCEDURE [dbo].[usp_MAS_GetActiveEmployeeItems]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT 
		id,
		lastname,
		firstname 
	FROM 
		employees WITH (NOLOCK)
	WHERE 
		status = 0 -- Active = 0, Disabled = 1
	ORDER BY 
		lastname
END
