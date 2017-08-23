
CREATE PROCEDURE [dbo].[usp_REL_UpdateTrafficProductDescription]
(
	@traffic_id		Int,
	@product_description_id	Int
)
AS

UPDATE traffic SET product_description_id = @product_description_id
WHERE
	traffic.id = @traffic_id
