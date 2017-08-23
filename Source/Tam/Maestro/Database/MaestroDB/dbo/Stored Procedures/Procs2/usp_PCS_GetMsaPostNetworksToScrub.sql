-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 5/29/2012
-- Description:	
-- =============================================
-- usp_PCS_GetMsaPostNetworksToScrub
CREATE PROCEDURE [dbo].[usp_PCS_GetMsaPostNetworksToScrub]
AS
BEGIN
	SELECT
		DISTINCT mpd.raw_network
	FROM
		msa_post_details mpd (NOLOCK)
	WHERE
		mpd.network_id IS NULL
	ORDER BY
		mpd.raw_network
END
