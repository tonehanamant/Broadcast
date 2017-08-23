-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 8/21/2013
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_STS2_GetSystemStatementGroupsByStatementType
	@statement_type TINYINT
AS
BEGIN
	SET NOCOUNT ON;

    SELECT
		sgs.*
	FROM
		dbo.system_statement_groups sgs (NOLOCK)
	WHERE
		sgs.statement_type=@statement_type
END
