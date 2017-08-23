-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 7/23/2010
-- Description:	Clears a posts affidavit aggregation.
-- =============================================
-- usp_PCS_ClearPostAggregations 16
CREATE PROCEDURE [dbo].[usp_PCS_ClearPostAggregations]
	@tam_post_proposal_id INT
AS
BEGIN
	BEGIN TRY
		DECLARE @media_month_id INT
		SELECT @media_month_id = p.posting_media_month_id FROM dbo.tam_post_proposals tpp (NOLOCK) JOIN dbo.proposals p (NOLOCK) ON p.id=tpp.posting_plan_proposal_id WHERE tpp.id=@tam_post_proposal_id;

		BEGIN TRANSACTION;

		-- reset tam_post_proposal aggregation status
		UPDATE
			tam_post_proposals
		SET
			aggregation_started=NULL,
			aggregation_completed=NULL,
			aggregation_duration=NULL,
			aggregation_status_code=0
		WHERE
			id=@tam_post_proposal_id;

		-- 1a) delete tam_post_system_detail_audiences
		DELETE 
			tpsda 
		FROM 
			tam_post_system_detail_audiences tpsda 
			JOIN tam_post_system_details tpsd (NOLOCK) ON tpsd.id=tpsda.tam_post_system_detail_id
			JOIN tam_post_proposals tpp (NOLOCK) ON tpp.id=tpsd.tam_post_proposal_id
		WHERE
			tpp.id = @tam_post_proposal_id

		-- 1b) delete tam_post_dma_detail_audiences
		DELETE 
			tpdda 
		FROM 
			tam_post_dma_detail_audiences tpdda 
			JOIN tam_post_dma_details tpdd (NOLOCK) ON tpdd.id=tpdda.tam_post_dma_detail_id
			JOIN tam_post_proposals tpp (NOLOCK) ON tpp.id=tpdd.tam_post_proposal_id
		WHERE
			tpp.id = @tam_post_proposal_id

		-- 1c) delete tam_post_network_detail_audiences
		DELETE 
			tpnda 
		FROM 
			tam_post_network_detail_audiences tpnda 
			JOIN tam_post_network_details tpnd (NOLOCK) ON tpnd.id=tpnda.tam_post_network_detail_id
			JOIN tam_post_proposals tpp (NOLOCK) ON tpp.id=tpnd.tam_post_proposal_id
		WHERE
			tpp.id = @tam_post_proposal_id

		-- 1d) delete tam_post_system_details
		DELETE 
			tpsd 
		FROM 
			tam_post_system_details tpsd 
			JOIN tam_post_proposals tpp (NOLOCK) ON tpp.id=tpsd.tam_post_proposal_id
		WHERE
			tpp.id = @tam_post_proposal_id

		-- 1e) delete tam_post_dma_details
		DELETE 
			tpdd 
		FROM 
			tam_post_dma_details tpdd 
			JOIN tam_post_proposals tpp (NOLOCK) ON tpp.id=tpdd.tam_post_proposal_id
		WHERE
			tpp.id = @tam_post_proposal_id

		-- 1f) delete tam_post_network_details
		DELETE 
			tpnd
		FROM 
			tam_post_network_details tpnd 
			JOIN tam_post_proposals tpp (NOLOCK) ON tpp.id=tpnd.tam_post_proposal_id
		WHERE
			tpp.id = @tam_post_proposal_id

		-- 1g) delete tam_post_affidavit_details
		DELETE 
			tpad
		FROM 
			dbo.tam_post_affidavit_details tpad 
		WHERE
			tpad.media_month_id=@media_month_id
			AND tpad.tam_post_proposal_id=@tam_post_proposal_id


		-- 2a) delete tam_post_analysis_reports_dayparts
		DELETE 
			tpard 
		FROM 
			tam_post_analysis_reports_dayparts tpard 
			JOIN tam_post_proposals tpp (NOLOCK) ON tpp.id=tpard.tam_post_proposal_id
		WHERE
			tpp.id = @tam_post_proposal_id

		-- 2b) delete tam_post_analysis_reports_dmas
		DELETE 
			tpard 
		FROM 
			tam_post_analysis_reports_dmas tpard 
			JOIN tam_post_proposals tpp (NOLOCK) ON tpp.id=tpard.tam_post_proposal_id
		WHERE
			tpp.id = @tam_post_proposal_id
			
		-- 2c) delete tam_post_analysis_reports_dma_networks
		DELETE 
			tpardn 
		FROM 
			tam_post_analysis_reports_dma_networks tpardn
			JOIN tam_post_proposals tpp (NOLOCK) ON tpp.id=tpardn.tam_post_proposal_id
		WHERE
			tpp.id = @tam_post_proposal_id
			
		-- 2d) delete tam_post_analysis_reports_iscis
		DELETE 
			tpari 
		FROM 
			tam_post_analysis_reports_iscis tpari
			JOIN tam_post_proposals tpp (NOLOCK) ON tpp.id=tpari.tam_post_proposal_id
		WHERE
			tpp.id = @tam_post_proposal_id
			
		-- 2e) delete tam_post_analysis_reports_isci_networks
		DELETE 
			tparin 
		FROM 
			tam_post_analysis_reports_isci_networks tparin
			JOIN tam_post_proposals tpp (NOLOCK) ON tpp.id=tparin.tam_post_proposal_id
		WHERE
			tpp.id = @tam_post_proposal_id

		-- 2f) delete tam_post_analysis_reports_isci_network_weeks
		DELETE 
			tparinw 
		FROM 
			tam_post_analysis_reports_isci_network_weeks tparinw
			JOIN tam_post_proposals tpp (NOLOCK) ON tpp.id=tparinw.tam_post_proposal_id
		WHERE
			tpp.id = @tam_post_proposal_id
			
		-- 2g) delete tam_post_analysis_reports_spots_per_weeks
		DELETE 
			tparspw 
		FROM 
			tam_post_analysis_reports_spots_per_weeks tparspw
			JOIN tam_post_proposals tpp (NOLOCK) ON tpp.id=tparspw.tam_post_proposal_id
		WHERE
			tpp.id = @tam_post_proposal_id
			
		-- 2h) delete tam_post_analysis_reports_isci_breakdowns
		DELETE 
			tparib 
		FROM 
			tam_post_analysis_reports_isci_breakdowns tparib
			JOIN tam_post_proposals tpp (NOLOCK) ON tpp.id=tparib.tam_post_proposal_id
		WHERE
			tpp.id = @tam_post_proposal_id

		-- 2i) delete tam_post_analysis_reports_grp_trp_dmas
		DELETE 
			tpargtd
		FROM 
			tam_post_analysis_reports_grp_trp_dmas tpargtd
			JOIN tam_post_proposals tpp (NOLOCK) ON tpp.id=tpargtd.tam_post_proposal_id
		WHERE
			tpp.id = @tam_post_proposal_id
			
		-- 2j) delete tam_post_exclusion_summary_audiences
		DELETE 
			tpesa 
		FROM 
			tam_post_exclusion_summary_audiences tpesa 
			JOIN tam_post_exclusion_summaries tpes (NOLOCK) ON tpes.id=tpesa.tam_post_exclusion_summary_id
			JOIN tam_post_proposals tpp (NOLOCK) ON tpp.id=tpes.tam_post_proposal_id
		WHERE
			tpp.id = @tam_post_proposal_id
				
		-- 2k) delete tam_post_exclusion_summaries
		DELETE 
			tpes
		FROM 
			tam_post_exclusion_summaries tpes
			JOIN tam_post_proposals tpp (NOLOCK) ON tpp.id=tpes.tam_post_proposal_id
		WHERE
			tpp.id = @tam_post_proposal_id
			

		-- 3) update tam_post_proposals
		UPDATE
			tam_post_proposals
		SET
			aggregation_status_code=0,
			aggregation_duration=NULL,
			aggregation_started=NULL,
			aggregation_completed=NULL
		WHERE
			id = @tam_post_proposal_id

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		ROLLBACK TRANSACTION;
        -- Re-raise the original error.
        EXEC wb.usp_RethrowError;
	END CATCH;
END
