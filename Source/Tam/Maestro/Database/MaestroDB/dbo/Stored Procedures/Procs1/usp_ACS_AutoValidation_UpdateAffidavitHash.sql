-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_ACS_AutoValidation_UpdateAffidavitHash]
	@id BIGINT,
	@hash CHAR(59),
	@media_month_id INT
AS
BEGIN
	UPDATE
		affidavits
	SET
		hash=@hash
	WHERE
		affidavits.media_month_id=@media_month_id
		AND id=@id
END
