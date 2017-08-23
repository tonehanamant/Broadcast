-- =============================================
-- Author:           Stephen DeFusco
-- Create date: 6/13/2014
-- Description:      Reads posted affidavit data for a given post.
--                         Unidentified DMA's are handled in code rather than this procedure.
-- Update:           10/22/2014 - added switch to use msa_deliveries (instead of affidavit_deliveries) when the tam_post_proposal record is of post_source_code=2 (msa)
--                                              also modified to switch use the tam_post_proposal.id field of the TAM Post when querying tam_post_affidavits instead of the @tam_post_proposal_id field passed in.
--                                              this was intentional as we DO NOT store tam_post_affidavits for the MSA tam_post_proposals.id, since they'd be equal to the TAM Posts tam_post_porposals.id anyway. this also gives us support for data already sent to MSA using the tam_post_proposal.ids of the TAM Posts.
--                         12/22/2015 - instead of recalculating units each time we now only refer to the already calculated and stored units in tam_post_affidavits.units.
--                                              this makes the math for dr_delivery and dr_eq_delivery much simpler.
--                         09/07/2016 - modified MSA part of the query to check if msa_deliveries.msa_material_id IS NOT NULL, and if it's not, use it in place of tam_post_affidavits.posted_material_id. This was done to have "View Post Results" in Proposal Composer match what MSA is reporting.
-- Reference: tam_post_proposals.post_source_code (0 = TAM Post, 1 = Fast Track, 2 = MSA)
-- =============================================
-- usp_PCS_ReadTamPostAffidavits 19052
CREATE PROCEDURE [dbo].[usp_PCS_ReadTamPostAffidavits]
       @tam_post_proposal_id INT
AS
BEGIN
       DECLARE @hh_audience_id INT
       SET @hh_audience_id = 31

       DECLARE @tam_post_id INT
       DECLARE @posting_plan_proposal_id INT    
       DECLARE @post_source_code TINYINT 
       DECLARE @media_month_id INT 
       DECLARE @default_rating_source_id TINYINT 
       DECLARE @effective_tam_post_proposal_id INT -- initially equal to @tam_post_proposal_id; if param @tam_post_proposal_id is post_source_code = MSA then this will be corresponding tam_post_proposal_id where post_source_code = TAM Post

       SET @effective_tam_post_proposal_id = @tam_post_proposal_id;

       -- set variables
       SELECT 
              @tam_post_id = tpp.tam_post_id,
              @posting_plan_proposal_id = tpp.posting_plan_proposal_id,
              @media_month_id = p.posting_media_month_id,
              @post_source_code = tpp.post_source_code
       FROM
              dbo.tam_post_proposals tpp (NOLOCK)
              JOIN dbo.proposals p (NOLOCK) ON p.id=tpp.posting_plan_proposal_id
       WHERE
              tpp.id=@tam_post_proposal_id;

       -- override @effective_tam_post_proposal_id if @post_source_code = MSA
       IF @post_source_code = 2 -- MSA
              SELECT
                     @effective_tam_post_proposal_id = tpp.id
              FROM
                     dbo.tam_post_proposals tpp (NOLOCK)
              WHERE
                     tpp.tam_post_id=@tam_post_id
                     AND tpp.posting_plan_proposal_id=@posting_plan_proposal_id
                     AND tpp.post_source_code=0 -- TAM Post

       CREATE TABLE #audiences (audience_id INT NOT NULL)
       INSERT INTO #audiences
              SELECT DISTINCT 
                     pa.audience_id 
              FROM 
                     dbo.tam_post_proposals tpp        (NOLOCK)
                     JOIN dbo.proposal_audiences pa    (NOLOCK) ON pa.proposal_id=tpp.posting_plan_proposal_id
              WHERE
                     tpp.tam_post_id=@tam_post_id;
                     
       CREATE TABLE #audiences_to_post (posting_plan_proposal_id INT NOT NULL, audience_id INT NOT NULL, rating_source_id TINYINT NOT NULL, default_rating_source_id TINYINT NULL)
       ALTER TABLE #audiences_to_post ADD CONSTRAINT [IX_audiences_to_post] PRIMARY KEY CLUSTERED 
       (
              posting_plan_proposal_id ASC,
              audience_id ASC
       );
       INSERT INTO #audiences_to_post
              SELECT DISTINCT 
                     tpp.posting_plan_proposal_id,
                     a.audience_id,
                     tp.rating_source_id,
                     rs.default_rating_source_id
              FROM
                     dbo.tam_post_proposals tpp        (NOLOCK)
                     JOIN dbo.tam_posts tp                    (NOLOCK) ON tp.id=tpp.tam_post_id
                     JOIN dbo.rating_sources rs        (NOLOCK) ON rs.id=tp.rating_source_id
                     CROSS APPLY #audiences a
              WHERE
                     tpp.id=@tam_post_proposal_id;

       CREATE TABLE #tam_post_affidavits (media_month_id INT NOT NULL, tam_post_proposal_id INT NOT NULL, id BIGINT NOT NULL, enabled BIT NOT NULL, affidavit_id BIGINT NOT NULL, proposal_detail_id INT NOT NULL, posted_material_id INT NOT NULL, affidavit_material_id INT NOT NULL, media_week_id INT NOT NULL, business_id INT NOT NULL, system_id INT NOT NULL, zone_id INT NOT NULL, affidavit_network_id INT NOT NULL, posted_network_id INT NOT NULL, dma_id INT NOT NULL, air_time INT NOT NULL, air_date DATETIME NOT NULL, rate MONEY NOT NULL, subscribers INT NOT NULL, units FLOAT NOT NULL, delivered_value MONEY NOT NULL)
       INSERT INTO #tam_post_affidavits
              SELECT
                     media_month_id, tam_post_proposal_id, id, enabled, affidavit_id, proposal_detail_id, posted_material_id, affidavit_material_id, media_week_id, business_id, system_id, zone_id, affidavit_network_id, posted_network_id, dma_id, air_time, air_date, rate, subscribers, units, delivered_value
              FROM
                     maestro_analysis.dbo.tam_post_affidavits tpa (NOLOCK)
              WHERE
                     tpa.media_month_id=@media_month_id
                     AND tpa.tam_post_proposal_id=@effective_tam_post_proposal_id;
       
       IF @post_source_code = 0 OR @post_source_code = 1 -- CADENT or FAST TRACK
       BEGIN
              SELECT
                     tpa.tam_post_proposal_id,
                     tpa.id,
                     tpa.proposal_detail_id,
                     au.audience_id,
                     tpa.posted_network_id,
                     tpa.business_id,
                     tpa.system_id,
                     tpa.posted_material_id,
                     tpa.affidavit_material_id,
                     tpa.air_time,
                     tpa.air_date,
                     tpa.media_week_id,
                     tpa.zone_id,
                     pd.spot_length_id,
                     tpa.dma_id,
                     tpa.enabled,
                     tpa.subscribers,
                     tpa.units,
                     CASE au.audience_id
                           WHEN @hh_audience_id THEN
                                  CASE
                                         WHEN ad_hh.audience_usage IS NOT NULL THEN
                                                (CAST(tpa.subscribers AS FLOAT) / ad_hh.universe) * ISNULL(ad_hh.regional_usage, ad_hh.audience_usage)
                                         ELSE
                                                (CAST(tpa.subscribers AS FLOAT) / ad_default_hh.universe) * ISNULL(ad_default_hh.regional_usage, ad_default_hh.audience_usage)
                                  END
                           ELSE
                                  CASE
                                         WHEN ad_demo.audience_usage IS NOT NULL THEN
                                                (CAST(tpa.subscribers AS FLOAT) / ad_hh.universe) * ISNULL(ad_demo.regional_usage, ad_demo.audience_usage)
                                         ELSE
                                                (CAST(tpa.subscribers AS FLOAT) / ad_default_hh.universe) * ISNULL(ad_default_demo.regional_usage, ad_default_demo.audience_usage)
                                  END
                     END 'delivery',
                     CASE au.audience_id
                           WHEN @hh_audience_id THEN
                                  CASE
                                         WHEN ad_hh.audience_usage IS NOT NULL THEN
                                                (CAST(tpa.subscribers AS FLOAT) / ad_hh.universe) * ISNULL(ad_hh.regional_usage, ad_hh.audience_usage)
                                         ELSE
                                                (CAST(tpa.subscribers AS FLOAT) / ad_default_hh.universe) * ISNULL(ad_default_hh.regional_usage, ad_default_hh.audience_usage)
                                  END
                           ELSE
                                  CASE
                                         WHEN ad_demo.audience_usage IS NOT NULL THEN
                                                (CAST(tpa.subscribers AS FLOAT) / ad_hh.universe) * ISNULL(ad_demo.regional_usage, ad_demo.audience_usage)
                                         ELSE
                                                (CAST(tpa.subscribers AS FLOAT) / ad_default_hh.universe) * ISNULL(ad_default_demo.regional_usage, ad_default_demo.audience_usage)
                                  END
                     END * sl.delivery_multiplier 'eq_delivery',
                     CASE au.audience_id
                           WHEN @hh_audience_id THEN
                                  tpa.units * pda.us_universe * pd.universal_scaling_factor * pda.rating
                           ELSE
                                  tpa.units * pda_hh.us_universe * pd.universal_scaling_factor * pda_hh.rating * pda.vpvh
                     END 'dr_delivery',
                     CASE au.audience_id
                           WHEN @hh_audience_id THEN
                                  tpa.units * pda.us_universe * pd.universal_scaling_factor * pda.rating
                           ELSE
                                  tpa.units * pda_hh.us_universe * pd.universal_scaling_factor * pda_hh.rating * pda.vpvh
                     END * sl.delivery_multiplier 'dr_eq_delivery'
              FROM
                     #tam_post_affidavits tpa                                             (NOLOCK)
                     JOIN dbo.proposal_details pd                                         (NOLOCK) ON pd.id=tpa.proposal_detail_id
                     JOIN dbo.materials m                                                 (NOLOCK) ON m.id=tpa.posted_material_id
                     JOIN dbo.spot_lengths sl                                             (NOLOCK) ON sl.id=m.spot_length_id
                     JOIN #audiences_to_post au                                                        ON au.posting_plan_proposal_id=@posting_plan_proposal_id
                     LEFT JOIN dbo.proposal_detail_audiences pda                   (NOLOCK) ON pda.proposal_detail_id=pd.id AND pda.audience_id=au.audience_id
                     LEFT JOIN dbo.proposal_detail_audiences pda_hh         (NOLOCK) ON pda_hh.proposal_detail_id=pd.id AND pda_hh.audience_id=@hh_audience_id
                     LEFT JOIN dbo.affidavit_deliveries ad_hh               (NOLOCK) ON ad_hh.media_month_id=@media_month_id                   AND ad_hh.rating_source_id=au.rating_source_id                                         AND ad_hh.audience_id=@hh_audience_id                  AND       ad_hh.affidavit_id=tpa.affidavit_id
                     LEFT JOIN dbo.affidavit_deliveries ad_demo                    (NOLOCK) ON ad_demo.media_month_id=@media_month_id                 AND ad_demo.rating_source_id=au.rating_source_id                               AND ad_demo.audience_id=au.audience_id              AND    ad_demo.affidavit_id=tpa.affidavit_id
                     LEFT JOIN dbo.affidavit_deliveries ad_default_hh       (NOLOCK) ON ad_default_hh.media_month_id=@media_month_id    AND ad_default_hh.rating_source_id=au.default_rating_source_id           AND ad_default_hh.audience_id=@hh_audience_id AND    ad_default_hh.affidavit_id=tpa.affidavit_id
                     LEFT JOIN dbo.affidavit_deliveries ad_default_demo     (NOLOCK) ON ad_default_demo.media_month_id=@media_month_id  AND ad_default_demo.rating_source_id=au.default_rating_source_id  AND ad_default_demo.audience_id=au.audience_id      AND    ad_default_demo.affidavit_id=tpa.affidavit_id
       END
       ELSE -- MSA
       BEGIN
              SELECT
                     @tam_post_proposal_id 'tam_post_proposal_id',
                     tpa.id,
                     tpa.proposal_detail_id,
                     au.audience_id,
                     tpa.posted_network_id,
                     tpa.business_id,
                     tpa.system_id,
                     m.id,
                     tpa.affidavit_material_id,
                     tpa.air_time,
                     tpa.air_date,
                     tpa.media_week_id,
                     tpa.zone_id,
                     pd.spot_length_id,
                     tpa.dma_id,
                     tpa.enabled,
                     tpa.subscribers,
                     tpa.units,
                     CASE md_hh.is_equivalized WHEN 0 THEN 
                           CASE au.audience_id WHEN @hh_audience_id THEN md_hh.delivery ELSE md_demo.delivery END 
                     ELSE 
                           CASE au.audience_id WHEN @hh_audience_id THEN md_hh.delivery ELSE md_demo.delivery END / sl.delivery_multiplier 
                     END 'delivery',
                     CASE md_hh.is_equivalized WHEN 1 THEN 
                           CASE au.audience_id WHEN @hh_audience_id THEN md_hh.delivery ELSE md_demo.delivery END 
                     ELSE 
                           CASE au.audience_id WHEN @hh_audience_id THEN md_hh.delivery ELSE md_demo.delivery END * sl.delivery_multiplier 
                     END 'eq_delivery',
                     CASE au.audience_id
                           WHEN @hh_audience_id THEN
                                  tpa.units * pda.us_universe * pd.universal_scaling_factor * pda.rating
                           ELSE
                                  tpa.units * pda_hh.us_universe * pd.universal_scaling_factor * pda_hh.rating * pda.vpvh
                     END 'dr_delivery',
                     CASE au.audience_id
                           WHEN @hh_audience_id THEN
                                  tpa.units * pda.us_universe * pd.universal_scaling_factor * pda.rating
                           ELSE
                                  tpa.units * pda_hh.us_universe * pd.universal_scaling_factor * pda_hh.rating * pda.vpvh
                     END * sl.delivery_multiplier 'dr_eq_delivery'
              FROM
                     #tam_post_affidavits tpa                                             (NOLOCK)
                     JOIN dbo.proposal_details pd                                         (NOLOCK) ON pd.id=tpa.proposal_detail_id
                     JOIN #audiences_to_post au                                                        ON au.posting_plan_proposal_id=@posting_plan_proposal_id
                     LEFT JOIN dbo.proposal_detail_audiences pda                   (NOLOCK) ON pda.proposal_detail_id=pd.id AND pda.audience_id=au.audience_id
                     LEFT JOIN dbo.proposal_detail_audiences pda_hh         (NOLOCK) ON pda_hh.proposal_detail_id=pd.id AND pda_hh.audience_id=@hh_audience_id
                     LEFT JOIN dbo.msa_deliveries md_hh                            (NOLOCK) ON md_hh.media_month_id=@media_month_id     AND md_hh.tam_post_proposal_id=@tam_post_proposal_id   AND       md_hh.tam_post_affidavit_id=tpa.id       AND md_hh.audience_id=@hh_audience_id    
                     LEFT JOIN dbo.msa_deliveries md_demo                          (NOLOCK) ON md_demo.media_month_id=@media_month_id   AND md_demo.tam_post_proposal_id=@tam_post_proposal_id AND       md_demo.tam_post_affidavit_id=tpa.id     AND md_demo.audience_id=au.audience_id
                     -- if MSA sent us back msa_deliveries.msa_material_id then use that (this is an override to what was actually posted in tam_post_affidavits.posted_material_id)
                     JOIN dbo.materials m                                                 (NOLOCK) ON m.id=CASE WHEN md_hh.msa_material_id IS NOT NULL THEN md_hh.msa_material_id ELSE tpa.posted_material_id END
                     JOIN dbo.spot_lengths sl                                             (NOLOCK) ON sl.id=m.spot_length_id
       END
       DROP TABLE #audiences;
       DROP TABLE #audiences_to_post;
       DROP TABLE #tam_post_affidavits;
END
