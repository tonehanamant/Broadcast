-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_STS2_GetSystemStatementGroupSystems
	@system_statement_group_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT
		system_statement_group_id,
		system_id
	FROM
		system_statement_group_systems (NOLOCK)
	WHERE
		system_statement_group_id=@system_statement_group_id
END
