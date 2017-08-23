-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_ACS_GetExcludedNetworks
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT
		n.*
	FROM
		networks n (NOLOCK)
		JOIN affidavit_import_excluded_networks aien ON aien.network_id=n.id
END
