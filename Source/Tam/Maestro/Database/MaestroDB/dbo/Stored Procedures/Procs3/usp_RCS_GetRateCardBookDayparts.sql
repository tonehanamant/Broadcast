-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 11/6/2012
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[usp_RCS_GetRateCardBookDayparts]
	@network_rate_card_book_id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT DISTINCT
		d.id,
		d.code,
		d.name,
		d.start_time,
		d.end_time,
		d.mon,
		d.tue,
		d.wed,
		d.thu,
		d.fri,
		d.sat,
		d.sun 
	FROM
		network_rate_cards nrc (NOLOCK)
		JOIN vw_ccc_daypart d ON d.id=nrc.daypart_id
	WHERE
		nrc.network_rate_card_book_id=@network_rate_card_book_id
END
