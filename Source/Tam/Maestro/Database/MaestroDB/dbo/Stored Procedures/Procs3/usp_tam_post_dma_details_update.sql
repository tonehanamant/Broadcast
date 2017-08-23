CREATE PROCEDURE [dbo].[usp_tam_post_dma_details_update]
(
	@id		Int,
	@enabled		Bit,
	@tam_post_proposal_id		Int,
	@dma_id		Int,
	@network_id		Int,
	@subscribers		BigInt,
	@units		Float,
	@total_spots		Int
)
AS
UPDATE tam_post_dma_details SET
	enabled = @enabled,
	tam_post_proposal_id = @tam_post_proposal_id,
	dma_id = @dma_id,
	network_id = @network_id,
	subscribers = @subscribers,
	units = @units,
	total_spots = @total_spots
WHERE
	id = @id
