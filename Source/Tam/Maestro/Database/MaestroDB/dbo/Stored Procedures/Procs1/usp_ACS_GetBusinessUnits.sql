-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 11/27/2012
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_ACS_GetBusinessUnits]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		bu.*
	FROM
		dbo.business_units bu (NOLOCK)
	WHERE
		bu.id IN (1,4,11)
END
