
CREATE Procedure [dbo].[usp_REL_GetAllReleaseProductDescriptions]

AS

SELECT
	id,
	product_description
FROM
	release_product_descriptions (NOLOCK)
order by product_description
