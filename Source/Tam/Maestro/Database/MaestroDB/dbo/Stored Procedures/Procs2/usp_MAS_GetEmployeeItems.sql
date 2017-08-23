-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_MAS_GetEmployeeItems
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
		employees 
	ORDER BY 
		lastname
END
