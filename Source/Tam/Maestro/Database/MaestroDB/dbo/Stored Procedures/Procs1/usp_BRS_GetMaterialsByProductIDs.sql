


CREATE PROCEDURE [dbo].[usp_BRS_GetMaterialsByProductIDs]
@IDs varchar(MAX)

AS
BEGIN

	SET NOCOUNT ON;
	SELECT
		id,
		spot_length_id,
		cmw_traffic_product_id,
		code,
		title,
		date_created,
		date_last_modified		
	FROM
		cmw_materials (nolock)
	WHERE
		cmw_traffic_product_id in (select id from dbo.SplitIntegers(@IDs))
	ORDER BY
		code ASC
END


