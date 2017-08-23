-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_GetDisplaySystemStatementGroups]
	@statement_type TINYINT
AS
BEGIN
	SET NOCOUNT ON;

    SELECT
		ssg.*
	FROM
		dbo.system_statement_groups ssg (NOLOCK)
	WHERE
		ssg.statement_type=@statement_type
	
	SELECT
		ssgs.system_statement_group_id,
		s.code
	FROM
		system_statement_group_systems ssgs (NOLOCK)
		JOIN dbo.system_statement_groups ssg (NOLOCK) ON ssg.id=ssgs.system_statement_group_id
			AND ssg.statement_type=@statement_type
		JOIN systems s (NOLOCK) ON s.id=ssgs.system_id
	ORDER BY
		s.code
END
