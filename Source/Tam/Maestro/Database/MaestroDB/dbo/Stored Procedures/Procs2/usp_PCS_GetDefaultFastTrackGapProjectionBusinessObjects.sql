﻿-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 12/16/2011
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_GetDefaultFastTrackGapProjectionBusinessObjects]
AS
BEGIN
	SELECT
		mm.*,
		br_dftgp.*,
		dr_dftgp.*
	FROM
		media_months mm (NOLOCK)
		JOIN default_fast_track_gap_projections br_dftgp (NOLOCK) ON br_dftgp.media_month_id=mm.id AND br_dftgp.rate_card_type_id=1
		JOIN default_fast_track_gap_projections dr_dftgp (NOLOCK) ON dr_dftgp.media_month_id=mm.id AND dr_dftgp.rate_card_type_id=2
	ORDER BY
		mm.[year] DESC,
		mm.[month] DESC
END
