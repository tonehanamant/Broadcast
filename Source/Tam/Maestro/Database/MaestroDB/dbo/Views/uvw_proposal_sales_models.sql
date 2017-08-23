
/****** Object:  View [dbo].[uvw_proposal_sales_models]    Script Date: 02/01/2011 11:33:40 ******/
CREATE VIEW [dbo].[uvw_proposal_sales_models]
AS
	SELECT
		p.id proposal_id,
		nrcb.sales_model_id sales_model_id
	FROM
		proposals p with(nolock)
		join network_rate_cards nrc with(nolock) on
			nrc.id = p.network_rate_card_id
		join network_rate_card_books nrcb with(nolock) on
			nrcb.id = nrc.network_rate_card_book_id;
