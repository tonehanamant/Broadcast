CREATE PROCEDURE usp_tam_post_dma_details_delete
(
	@id Int
)
AS
DELETE FROM tam_post_dma_details WHERE id=@id
