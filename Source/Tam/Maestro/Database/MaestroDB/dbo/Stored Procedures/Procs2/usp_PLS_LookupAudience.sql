-- =============================================
-- Author:Stephen DeFusco
-- Create date: 5/7/2013
-- Description:
-- =============================================
CREATE PROCEDURE [dbo].[usp_PLS_LookupAudience]
	@name VARCHAR(31)
AS
BEGIN
	SET NOCOUNT ON;

    SELECT
		a.*
	FROM
		dbo.audiences a (NOLOCK)
	WHERE
		a.code=@name
END
