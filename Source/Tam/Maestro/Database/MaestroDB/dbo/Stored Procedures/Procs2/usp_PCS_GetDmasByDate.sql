-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_PCS_GetDmasByDate
	@effective_date DATETIME
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT
		dma_id,
		code,
		[name],
		[rank],
		tv_hh,
		cable_hh,
		active,
		start_date,
		flag
	FROM
		uvw_dma_universe d (NOLOCK)
	WHERE
		d.start_date<=@effective_date AND (d.end_date>=@effective_date OR d.end_date IS NULL)
END
