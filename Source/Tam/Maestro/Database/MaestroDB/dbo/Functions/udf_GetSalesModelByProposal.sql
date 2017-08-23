
-- =============================================
-- Author:		David Sisson
-- Create date: 03/16/2009
-- Description:	Returns dma_histories as of specified date.
-- =============================================
CREATE FUNCTION [dbo].[udf_GetSalesModelByProposal]
(	
	@idProposal as int
)
RETURNS TABLE
AS
RETURN 
(
	SELECT
		p.id proposal_id,
		nrcb.sales_model_id sales_model_id
	FROM
		proposals p with(nolock)
		join network_rate_cards nrc with(nolock) on
			nrc.id = p.network_rate_card_id
		join network_rate_card_books nrcb with(nolock) on
			nrcb.id = nrc.network_rate_card_book_id
	WHERE
		@idProposal = p.id
);

