-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_ACS_AutoValidation_UpdateAffidavitMaterialId]
	@media_month_id INT,
	@material_id INT,
	@affidavit_copy VARCHAR(63)
AS
BEGIN
	UPDATE
		affidavits
	SET
		material_id=@material_id
	FROM
		affidavits
	WHERE
		affidavits.media_month_id=@media_month_id
		AND affidavits.affidavit_copy=@affidavit_copy
		AND affidavits.material_id IS NULL
END
