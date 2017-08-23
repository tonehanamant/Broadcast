

-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 5/24/2012
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PCS_DeleteMsaPost]
	@tam_post_id INT,
	@file_name VARCHAR(255),
	@media_month_id INT
AS
BEGIN
	CREATE TABLE #ids(id INT)
	INSERT into #ids
		SELECT id FROM msa_posts mp (NOLOCK) WHERE mp.tam_post_id=@tam_post_id AND mp.file_name=@file_name and mp.media_month_id=@media_month_id
	
	DELETE FROM msa_post_daypart_details WHERE media_month_id=@media_month_id and tam_post_id=@tam_post_id and msa_post_id in (select id from #ids)
	DELETE FROM msa_post_weekly_details WHERE media_month_id=@media_month_id and tam_post_id=@tam_post_id and msa_post_id in (select id from #ids)
	DELETE FROM msa_post_isci_details WHERE media_month_id=@media_month_id and tam_post_id=@tam_post_id and msa_post_id in (select id from #ids)
	DELETE FROM msa_posts WHERE id in (select id from #ids) and  media_month_id=@media_month_id and tam_post_id=@tam_post_id
END
