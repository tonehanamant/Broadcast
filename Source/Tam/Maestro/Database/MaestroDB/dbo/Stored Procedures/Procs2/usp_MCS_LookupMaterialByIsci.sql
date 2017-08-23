-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 10/5/2011
-- Description:	Looks up material by isci.
-- =============================================
CREATE PROCEDURE usp_MCS_LookupMaterialByIsci
	@isci VARCHAR(31)
AS
BEGIN
	SELECT
		m.*
	FROM
		materials m (NOLOCK)
	WHERE
		m.code=@isci
END
