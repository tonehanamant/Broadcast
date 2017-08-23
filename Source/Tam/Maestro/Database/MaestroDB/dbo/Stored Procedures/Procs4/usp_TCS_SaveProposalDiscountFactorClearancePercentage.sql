-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 7/2/2014
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_TCS_SaveProposalDiscountFactorClearancePercentage
	@property_id INT,
	@value VARCHAR(255)
AS
BEGIN
	SET NOCOUNT ON;
	
	UPDATE dbo.properties SET value=@value WHERE id=@property_id;
END
