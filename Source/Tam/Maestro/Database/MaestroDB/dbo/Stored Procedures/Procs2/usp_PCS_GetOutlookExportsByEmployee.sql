-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 9/23/2011
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_PCS_GetOutlookExportsByEmployee
	@employee_id INT
AS
BEGIN
	SELECT
		oe.*
	FROM
		outlook_exports oe (NOLOCK)
	WHERE
		oe.employee_id=@employee_id
END
