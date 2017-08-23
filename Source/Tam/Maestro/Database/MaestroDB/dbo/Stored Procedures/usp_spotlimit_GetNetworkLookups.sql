/* END Dead Code Cleanup */

CREATE PROCEDURE [dbo].[usp_spotlimit_GetNetworkLookups]
	@current_date DATETIME
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
 
DECLARE @current_year INT;
DECLARE @current_quarter TINYINT;
 
SELECT
      @current_year = mm.year,
      @current_quarter = 
	  CASE mm.month 
		  WHEN 1 THEN 1 
		  WHEN 2 THEN 1 
		  WHEN 3 THEN 1 
		  WHEN 4 THEN 2 
		  WHEN 5 THEN 2 
		  WHEN 6 THEN 2 
		  WHEN 7 THEN 3 
		  WHEN 8 THEN 3 
		  WHEN 9 THEN 3 
		  WHEN 10 THEN 4 
		  WHEN 11 THEN 4 
		  WHEN 12 THEN 4 
	   END
FROM
      media_months mm
WHERE
      @current_date BETWEEN mm.start_date AND mm.end_date
 
DECLARE @network_ids TABLE (network_id INT);
INSERT INTO @network_ids
      SELECT
            nrcd.network_id
      FROM
            network_rate_card_books nrcb (NOLOCK)
            JOIN network_rate_cards nrc (NOLOCK) ON nrc.network_rate_card_book_id=nrcb.id
            JOIN network_rate_card_details nrcd (NOLOCK) ON nrcd.network_rate_card_id=nrc.id
            JOIN networks n (NOLOCK) ON n.id=nrcd.network_id
      WHERE (nrcb.year>@current_year OR (nrcb.year=@current_year OR nrcb.quarter>=@current_quarter))
      GROUP BY
            nrcd.network_id
 
SELECT
      n.id as Id,
	  n.code as 'Display'
FROM
      @network_ids nids
      JOIN networks n ON n.id=nids.network_id
ORDER BY
      n.code
END