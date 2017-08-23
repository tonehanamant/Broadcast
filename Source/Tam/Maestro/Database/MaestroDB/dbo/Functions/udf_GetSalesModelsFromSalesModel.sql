-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 3/25/2014
-- Description:	<Description,,>
-- =============================================
CREATE FUNCTION [dbo].[udf_GetSalesModelsFromSalesModel]
(	
	@sales_model_id INT
)
RETURNS @return TABLE
(
	id INT
)
AS
BEGIN
	IF @sales_model_id = 1 OR @sales_model_id = 6
		INSERT INTO @return
			SELECT
				sm.id
			FROM
				sales_models sm (NOLOCK)
			WHERE
				sm.id IN (1,6)
	ELSE
		INSERT INTO @return
			SELECT @sales_model_id
	RETURN
END
