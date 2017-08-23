

-- =============================================
-- Author:		Nicholas Kheynis
-- Create date: 3/19/2014
-- Description:	
-- =============================================
-- usp_MSA_GetMSAFilesByMediaMonthId 382
CREATE PROCEDURE [dbo].[usp_MSA_GetMSAFilesByMediaMonthId]
	@media_month_id INT
AS
BEGIN
	SET NOCOUNT ON;

    SELECT
		mp_hh.tam_post_id,
		mp_hh.file_name,
		mp_hh.agency,
		mp_hh.advertiser,
		mp_hh.product,
		mp_hh.source,
		SUM(mp_hh.total_delivered) 'total_hh_delivered',
		SUM(ISNULL(mp_demo.total_delivered,0)) 'total_demo_delivered',
		mp_hh.date_started,
		mp_hh.media_month_id
	FROM
		dbo.msa_posts mp_hh (NOLOCK)
		LEFT JOIN dbo.msa_posts mp_demo (NOLOCK) ON mp_demo.media_month_id=mp_hh.media_month_id
			AND mp_demo.tam_post_id=mp_hh.tam_post_id
			AND mp_demo.file_name=mp_hh.file_name
	WHERE
		mp_hh.media_month_id = @media_month_id
		AND mp_hh.audience_id=31
	GROUP BY 
		mp_hh.tam_post_id,
		mp_hh.file_name,
		mp_hh.agency,
		mp_hh.advertiser,
		mp_hh.product,
		mp_hh.source,
		mp_hh.date_started,
		mp_hh.media_month_id
	ORDER BY
		mp_hh.file_name DESC
END
