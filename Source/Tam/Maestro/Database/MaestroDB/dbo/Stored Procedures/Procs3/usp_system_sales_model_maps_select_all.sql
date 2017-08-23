﻿
CREATE PROCEDURE [dbo].[usp_system_sales_model_maps_select_all]
AS
SELECT
	id,
	system_id,
	sales_model_id,
	map_set,
	map_value,
	effective_date
FROM
	system_sales_model_maps WITH(NOLOCK)

