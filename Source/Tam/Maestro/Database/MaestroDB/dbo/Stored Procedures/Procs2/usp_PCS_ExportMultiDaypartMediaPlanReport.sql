


--exec usp_PCS_ExportMultiDaypartMediaPlanReport 21985

CREATE procedure [dbo].[usp_PCS_ExportMultiDaypartMediaPlanReport]
(
	@ProposalId int
)
AS
BEGIN
	DECLARE
		@audience_id31 int,
		@audience_id248 int

	SET @audience_id31 = (SELECT audience_id FROM proposal_audiences (NOLOCK) WHERE ordinal=0 AND proposal_id=@ProposalId)
	SET @audience_id248 = (SELECT audience_id FROM proposal_audiences (NOLOCK) WHERE ordinal=1 AND proposal_id=@ProposalId)

	DECLARE @MediaPlanReport_Aid31 AS TABLE
	(
		[original_proposal_id_31] [int] NULL,
		[advertiser_31] [varchar](63)  NULL,
		[product_31] [varchar](127) NULL,
		[agency_31] [varchar](63) NOT NULL,
		[title_31] [varchar](127) NOT NULL,
		[flight_text_31] [varchar](1027) NOT NULL,
		[date_created_31] [datetime] NULL,
		[version_number_31] [varchar](127) NOT NULL,
		[id_31] [int] NOT NULL,
		[code_31] [varchar](15) NOT NULL,
		[length_31] [int] NOT NULL,
		[num_spots_31] [int] NOT NULL,
		[proposal_rate_31] [money] NULL,
		[total_cost_31] [numeric](38, 6) NULL,
		[daypart_text_31] [varchar](63) NOT NULL,
		[proposal_detail_id_31] [int] NOT NULL,
		[audience_id_31] [int] NOT NULL,
		[coverage_universe_31] [float] NULL,
		[rating_31] [float] NOT NULL,
		[delivery_31] [float] NULL,
		[cpm_31] [money] NULL,
		[vpvh_31] [float] NULL,
		[neq_rtg_31] [float] NULL,
		[audience_name_31] [varchar](127) NULL,
		[audience_us_universe_31] [float] NULL,
		[is_equivalized_31] [bit] NOT NULL,
		[c3_bias_enabled_31] [bit] NOT NULL,
		[network_universe_31] [float] NOT NULL,
		[buyer_note] [varchar](2047) NULL,
		[print_title] [varchar](2047) NULL
	) 

	insert into @MediaPlanReport_Aid31
	exec	usp_PCS_ExportMultiDaypartMediaPlanReportDetail @ProposalId,@audience_id31 


	DECLARE @MediaPlanReport_Aid248 AS TABLE
	(
		[original_proposal_id_248] [int] NULL,
		[advertiser_248] [varchar](63)  NULL,
		[product_248] [varchar](127) NULL,
		[agency_248] [varchar](63) NOT NULL,
		[title_248] [varchar](127) NOT NULL,
		[flight_text_248] [varchar](1027) NOT NULL,
		[date_created_248] [datetime] NULL,
		[version_number_248] [varchar](127) NOT NULL,
		[id_248] [int] NOT NULL,
		[code_248] [varchar](15) NOT NULL,
		[length_248] [int] NOT NULL,
		[num_spots_248] [int] NOT NULL,
		[proposal_rate_248] [money] NULL,
		[total_cost_248] [numeric](38, 6) NULL,
		[daypart_text_248] [varchar](63) NOT NULL,
		[proposal_detail_id_248] [int] NOT NULL,
		[audience_id_248] [int] NOT NULL,
		[coverage_universe_248] [float] NULL,
		[rating_248] [float] NOT NULL,
		[delivery_248] [float] NULL,
		[cpm_248] [money] NULL,
		[vpvh_248] [float] NULL,
		[neq_rtg_248] [float] NULL,
		[audience_name_248] [varchar](127) NULL,
		[audience_us_universe_248] [float] NULL,
		[is_equivalized_248] [bit] NOT NULL,
		[c3_bias_enabled_248] [bit] NOT NULL,
		[network_universe_248] [float] NOT NULL,
		[buyer_note] [varchar](2047) NULL,
		[print_title] [varchar](2047) NULL
	) 

	insert into @MediaPlanReport_Aid248
	exec	usp_PCS_ExportMultiDaypartMediaPlanReportDetail @ProposalId,@audience_id248 
	
	IF (SELECT COUNT(*) FROM @MediaPlanReport_Aid248) > 0
		SELECT
			DISTINCT t31.*,t248.*
		FROM
			@MediaPlanReport_Aid31 t31
			JOIN @MediaPlanReport_Aid248 t248 ON t31.id_31 = t248.id_248
		ORDER BY
			t31.code_31
	ELSE
		SELECT 
			DISTINCT t31.*
		FROM
			@MediaPlanReport_Aid31 t31
		ORDER BY
			t31.code_31
END


