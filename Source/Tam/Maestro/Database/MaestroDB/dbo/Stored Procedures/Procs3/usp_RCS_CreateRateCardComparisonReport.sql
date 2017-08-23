-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
-- usp_RCS_CreateRateCardComparisonReport 79,1,1,54,1,1
CREATE PROCEDURE usp_RCS_CreateRateCardComparisonReport
	@network_rate_card_book_id1 INT,
	@daypart_id1 INT,
	@rate_card_type_id1 INT,

	@network_rate_card_book_id2 INT,
	@daypart_id2 INT,
	@rate_card_type_id2 INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    EXEC usp_RCS_GetRateCardComparisonReport @network_rate_card_book_id1,@daypart_id1,@rate_card_type_id1
	EXEC usp_RCS_GetRateCardComparisonReport @network_rate_card_book_id2,@daypart_id2,@rate_card_type_id2
END
