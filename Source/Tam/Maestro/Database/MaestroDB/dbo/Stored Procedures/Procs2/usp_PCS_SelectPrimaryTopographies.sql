
CREATE PROCEDURE [dbo].[usp_PCS_SelectPrimaryTopographies]
	@sales_model_id INT
AS
BEGIN
	SELECT	
		id,
		name 
	FROM 
		topographies (NOLOCK)
	WHERE 
		id IN (SELECT topography_id FROM sales_model_topographies (NOLOCK) WHERE sales_model_id=@sales_model_id)
		AND topography_type=0
	ORDER BY 
		name
END
