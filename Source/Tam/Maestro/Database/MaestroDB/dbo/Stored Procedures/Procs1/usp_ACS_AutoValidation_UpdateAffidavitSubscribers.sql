-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_ACS_AutoValidation_UpdateAffidavitSubscribers]
	@id BIGINT,
	@subscribers INT,
	@media_month_id INT
AS
BEGIN
	UPDATE
		affidavits
	SET
		subscribers=@subscribers
	WHERE
		affidavits.media_month_id=@media_month_id
		AND affidavits.id=@id
END
