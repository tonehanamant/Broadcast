-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_BRS_GetOrderEditors
@OrderID int

AS
BEGIN

	SET NOCOUNT ON;

	SELECT DISTINCT
		employee_id
	FROM
		cmw_traffic_employees (nolock)
	WHERE
		cmw_traffic_id = @OrderID
END
