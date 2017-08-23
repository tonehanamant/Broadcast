-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 5/23/2012
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_PCS_LookupIsci
	@isci VARCHAR(127)
AS
BEGIN
	SELECT
		m.*
	FROM
		materials m (NOLOCK)
	WHERE
		m.code=@isci
END
