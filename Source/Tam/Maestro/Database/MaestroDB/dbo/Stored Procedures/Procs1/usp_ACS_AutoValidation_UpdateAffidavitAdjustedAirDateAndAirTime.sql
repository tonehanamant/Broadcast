-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 1/14/2013
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_ACS_AutoValidation_UpdateAffidavitAdjustedAirDateAndAirTime]
	@id BIGINT,
	@media_month_id INT,
	@adjusted_air_date DATE,
	@adjusted_air_time INT
AS
BEGIN
	UPDATE
		affidavits
	SET
		adjusted_air_date=@adjusted_air_date,
		adjusted_air_time=@adjusted_air_time		
	WHERE
		affidavits.media_month_id=@media_month_id
		AND affidavits.id=@id
END
