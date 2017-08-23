-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 3/13/2013
-- Description:	Delete Nielsen NAD data for the given quarter.
-- =============================================
CREATE PROCEDURE [dbo].[usp_ARS_DeleteNielsenNadData]
	@start_date DATE,
	@end_date DATE
AS
BEGIN
	DELETE FROM 
		dbo.nielsen_nads
	WHERE 
		start_date=@start_date 
		AND end_date=@end_date 
END
