-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 7/2/2014
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_TCS_SaveSalesModelTrafficVig
	@id INT,
	@value FLOAT
AS
BEGIN
	SET NOCOUNT ON;
	
	UPDATE dbo.sales_model_traffic_vigs SET value=@value WHERE id=@id;
END
