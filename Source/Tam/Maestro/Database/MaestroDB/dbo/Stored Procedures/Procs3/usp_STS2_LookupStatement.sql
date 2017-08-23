-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_STS2_LookupStatement]
	@media_month_id INT,
	@statement_type TINYINT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT
		id,
		media_month_id,
		statement_type
	FROM
		statements (NOLOCK)
	WHERE
		media_month_id = @media_month_id
		AND statement_type = @statement_type
END
