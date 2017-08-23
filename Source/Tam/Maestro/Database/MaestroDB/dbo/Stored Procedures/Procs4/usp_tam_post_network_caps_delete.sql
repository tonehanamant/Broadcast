CREATE PROCEDURE [dbo].[usp_tam_post_network_caps_delete]
(
      @tam_post_id            Int,
      @network_id       Int)
AS
DELETE FROM
      dbo.tam_post_network_caps
WHERE
      tam_post_id = @tam_post_id
AND
      network_id = @network_id
