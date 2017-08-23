﻿CREATE PROCEDURE [dbo].[usp_traffic_material_flights_select]
(
	@id Int
)
AS
SELECT
	id,
	traffic_material_id,
	start_date,
	end_date,
	selected
FROM
	traffic_material_flights (NOLOCK)
WHERE
	id = @id
