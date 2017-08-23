﻿
CREATE PROCEDURE [dbo].[usp_system_image_sales_model_maps_select_all]
AS
SELECT
	id,
	system_id,
	image_id,
	sales_model_id,
	map_set,
	effective_date
FROM
	system_image_sales_model_maps WITH(NOLOCK)

