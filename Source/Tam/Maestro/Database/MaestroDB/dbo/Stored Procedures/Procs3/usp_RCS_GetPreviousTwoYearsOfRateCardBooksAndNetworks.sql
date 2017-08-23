-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 11/16/2013
-- Description:	<Description,,>
-- =============================================
-- usp_RCS_GetPreviousTwoYearsOfRateCardBooksAndNetworks
CREATE PROCEDURE [dbo].[usp_RCS_GetPreviousTwoYearsOfRateCardBooksAndNetworks]
AS
BEGIN
	SET NOCOUNT ON;

    CREATE TABLE #books (id INT)
	INSERT INTO #books
		SELECT 
			nrcb.id
		FROM 
			network_rate_card_books nrcb (NOLOCK)
		WHERE
			nrcb.[year] IN (
				SELECT DISTINCT TOP 2 [year] FROM network_rate_card_books (NOLOCK) WHERE date_approved IS NOT NULL ORDER BY [year] DESC
			)
			AND date_approved IS NOT NULL
			
	SELECT 
		nrcb.*
	FROM 
		#books b
		JOIN network_rate_card_books nrcb (NOLOCK) ON nrcb.id=b.id
	ORDER BY
		nrcb.sales_model_id,
		nrcb.[year] DESC,
		nrcb.[quarter] DESC,
		nrcb.id DESC
		
	SELECT DISTINCT
		nrcb.id,
		nrcd.network_id
	FROM
		#books b
		JOIN network_rate_card_books nrcb (NOLOCK) ON nrcb.id=b.id
		JOIN network_rate_cards nrc (NOLOCK) ON nrc.network_rate_card_book_id=nrcb.id
		JOIN network_rate_card_details nrcd (NOLOCK) ON nrcd.network_rate_card_id=nrc.id
		
	DROP TABLE #books;
END
