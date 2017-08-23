-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_IsProposalInSalesModel]
	@sales_model_id INT,
	@proposal_id INT
AS
BEGIN
	SELECT 
		COUNT(1) 
	FROM 
		proposal_sales_models (NOLOCK) 
	WHERE 
		proposal_id=@proposal_id 
		AND sales_model_id IN (SELECT id FROM dbo.udf_GetSalesModelsFromSalesModel(@sales_model_id))
END
