-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 6/15/2015
-- Description:	Used to populate MSA Export file "contract_details.txt"
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_MsaExport_ContractDetails]
	@msa_tam_post_proposal_ids UniqueIdTable READONLY
AS
BEGIN
	SET NOCOUNT ON;
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

    SELECT
		pd.id 'id',
		tpp.tam_post_id 'con_id',
		pd.id 'line_number',
		'S' 'type',
		pd.network_id 'net_id',
		sl.length 'length',
		CONVERT(VARCHAR(10), pd.start_date, 120) 'start_date', 
		CONVERT(VARCHAR(10), pd.end_date, 120) 'end_date',
		d.code 'dp_code',
		d.name 'daypart',
		d.start_time 'start_time',
		d.end_time 'end_time',
		d.mon 'mon',
		d.tue 'tue',
		d.wed 'wed',
		d.thu 'thu',
		d.fri 'fri',
		d.sat 'sat',
		d.sun 'sun'
	FROM
		@msa_tam_post_proposal_ids mttp
		JOIN tam_post_proposals tpp ON tpp.id=mttp.id
		JOIN proposal_details pd ON pd.proposal_id=tpp.posting_plan_proposal_id
		JOIN spot_lengths sl ON sl.id=pd.spot_length_id
		JOIN vw_ccc_daypart d ON pd.daypart_id=d.id
	ORDER BY
		tpp.tam_post_id,
		pd.id
END