CREATE PROCEDURE usp_tam_post_dma_details_select
(
	@id Int
)
AS
SELECT
	*
FROM
	tam_post_dma_details WITH(NOLOCK)
WHERE
	id = @id
