CREATE PROCEDURE usp_topography_dmas_select
(
	@topography_id		Int,
	@dma_id		Int
)
AS
SELECT
	*
FROM
	topography_dmas WITH(NOLOCK)
WHERE
	topography_id=@topography_id
	AND
	dma_id=@dma_id

