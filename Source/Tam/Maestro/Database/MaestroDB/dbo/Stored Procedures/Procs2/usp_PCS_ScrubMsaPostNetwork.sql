

-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 5/29/2012
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_ScrubMsaPostNetwork]
	@network VARCHAR(127),
	@network_id INT
AS
BEGIN
	IF (SELECT COUNT(nm.id) FROM network_maps nm (NOLOCK) WHERE nm.map_set='MSA' AND nm.map_value=@network) = 0
		BEGIN
			INSERT INTO network_maps
				SELECT @network_id,'MSA',@network,1,NULL,'1970-01-01 00:00:00.000'
		END
	ELSE
		BEGIN
			UPDATE network_maps SET map_value=@network WHERE map_set='MSA' AND map_value=@network
		END
END
