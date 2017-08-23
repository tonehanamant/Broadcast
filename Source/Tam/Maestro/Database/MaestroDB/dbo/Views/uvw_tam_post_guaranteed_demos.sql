
/****** Object:  View [dbo].[uvw_tam_post_guaranteed_demos]    Script Date: 9/17/2014 2:12:24 PM ******/
CREATE VIEW [dbo].[uvw_tam_post_guaranteed_demos]
AS
       SELECT
              flattened.tam_post_id,
              flattened.audience_id  'guaranteed_audience_id',
              a.code 'guaranteed_audience_code'
       FROM (
              SELECT
                     tpp.tam_post_id,
                     MIN(pa.audience_id) 'audience_id'
              FROM
                     dbo.tam_post_proposals tpp (NOLOCK)
                     JOIN dbo.tam_posts tp (NOLOCK) ON tp.id=tpp.tam_post_id
                           AND tp.is_deleted=0
                     JOIN proposals p (NOLOCK) ON p.id=tpp.posting_plan_proposal_id
                     JOIN proposal_audiences pa (NOLOCK) ON pa.proposal_id=tpp.posting_plan_proposal_id
                           AND pa.ordinal=p.guarantee_type
              GROUP BY
                     tpp.tam_post_id
       ) flattened
       JOIN audiences a (NOLOCK) ON a.id=flattened.audience_id
