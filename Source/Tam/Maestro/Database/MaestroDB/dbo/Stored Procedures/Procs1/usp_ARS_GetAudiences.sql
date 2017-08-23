-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 7/11/2011
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[usp_ARS_GetAudiences]
AS
BEGIN
	SELECT
		a.*
	FROM
		audiences a (NOLOCK)
END
