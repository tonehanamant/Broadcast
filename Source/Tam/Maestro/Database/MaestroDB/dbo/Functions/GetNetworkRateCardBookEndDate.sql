
-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[GetNetworkRateCardBookEndDate]
(
	@network_rate_card_book_id INT
)
RETURNS DATETIME
AS
BEGIN
	DECLARE @return DATETIME
	DECLARE @year INT
	DECLARE @quarter INT
	DECLARE @media_month_id INT

	SELECT 
		@year=year, 
		@quarter=quarter, 
		@media_month_id=media_month_id 
	FROM 
		network_rate_card_books nrcb (NOLOCK) 
	WHERE 
		id=@network_rate_card_book_id

	IF @media_month_id IS NULL
		BEGIN
			SET @return = (
				SELECT MAX(end_date) FROM media_months mm (NOLOCK) WHERE CASE mm.month WHEN 1 THEN 1 WHEN 2 THEN 1 WHEN 3 THEN 1 WHEN 4 THEN 2 WHEN 5 THEN 2 WHEN 6 THEN 2 WHEN 7 THEN 3 WHEN 8 THEN 3 WHEN 9 THEN 3 WHEN 10 THEN 4 WHEN 11 THEN 4 WHEN 12 THEN 4 END = @quarter AND year=@year
			)
		END
	ELSE
		BEGIN
			SET @return = (
				SELECT end_date FROM media_months mm (NOLOCK) WHERE id=@media_month_id
			)
		END

	RETURN @return
END

