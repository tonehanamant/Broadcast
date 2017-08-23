
CREATE PROCEDURE [dbo].[usp_PCS_GetDefaultTopographies]
	@sales_model_id INT
AS
BEGIN
	SELECT
		id,
		code,
		name,
		topography_type
	FROM
		topographies (NOLOCK) 
	WHERE
		id IN (SELECT topography_id FROM sales_model_topographies (NOLOCK) WHERE sales_model_id=@sales_model_id AND is_default=1)
END

