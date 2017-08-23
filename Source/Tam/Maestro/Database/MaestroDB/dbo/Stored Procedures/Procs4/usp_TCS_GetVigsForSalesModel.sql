/****** Object:  Table [dbo].[usp_TCS_GetVigsForSalesModel]    Script Date: 11/16/2012 14:51:25 ******/
CREATE PROCEDURE [dbo].[usp_TCS_GetVigsForSalesModel]
(
	@sales_model_id int
)
AS
	SELECT
		map_name,
		value 
	FROM
		sales_model_traffic_vigs (NOLOCK)
	WHERE
		sales_model_id = @sales_model_id
