-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_ACS_AutoValidation_UpdateAffidavitTrafficIds]
	@material_id INT,
	@invoice_id INT,
	@traffic_id INT
AS
BEGIN
	UPDATE
		affidavits
	SET
		traffic_id=@traffic_id
	WHERE
		traffic_id IS NULL
		AND invoice_id=@invoice_id
		AND material_id=@material_id
END
