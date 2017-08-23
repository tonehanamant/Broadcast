-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_GetSystemStatements]
	@system_statement_group_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT
		ss.*
	FROM
		system_statements ss (NOLOCK)
	WHERE
		ss.system_statement_group_id = @system_statement_group_id
END
