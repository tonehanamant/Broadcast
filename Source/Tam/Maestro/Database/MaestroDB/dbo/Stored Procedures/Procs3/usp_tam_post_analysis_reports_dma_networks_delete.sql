﻿CREATE PROCEDURE [dbo].[usp_tam_post_analysis_reports_dma_networks_delete]
(
	@tam_post_proposal_id		Int,
	@network_id		Int,
	@audience_id		Int,
	@enabled		Bit)
AS
DELETE FROM
	tam_post_analysis_reports_dma_networks
WHERE
	tam_post_proposal_id = @tam_post_proposal_id
 AND
	network_id = @network_id
 AND
	audience_id = @audience_id
 AND
	enabled = @enabled
