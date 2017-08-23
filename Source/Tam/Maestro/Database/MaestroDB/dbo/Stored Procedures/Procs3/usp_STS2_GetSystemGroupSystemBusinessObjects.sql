-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_STS2_GetSystemGroupSystemBusinessObjects
	@system_statement_group_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT
		ssgs.system_statement_group_id,
		ssgs.system_id,
		s.code
	FROM
		system_statement_group_systems ssgs (NOLOCK)
		JOIN systems s (NOLOCK) ON s.id=ssgs.system_id
	WHERE
		ssgs.system_statement_group_id=@system_statement_group_id
END
