-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 7/22/2011
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetTamPostExcludedAffidavitSystems]
	@tam_post_excluded_affidavit_id INT
AS
BEGIN
	SELECT
		tpeas.*
	FROM
		tam_post_excluded_affidavit_systems tpeas (NOLOCK)
	WHERE
		tpeas.tam_post_excluded_affidavit_id = @tam_post_excluded_affidavit_id
END
