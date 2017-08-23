-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 11/30/2012
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_STS2_GetYearsOfSystemStatements
	@statement_type TINYINT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		DISTINCT mm.[year]
	FROM
		statements s (NOLOCK)
		JOIN dbo.media_months mm (NOLOCK) ON mm.id=s.media_month_id
	WHERE
		s.statement_type=@statement_type
	ORDER BY
		mm.[year] DESC
END
