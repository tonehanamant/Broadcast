-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_MAS_GetQueuedEmailOutboxAttachments]
	@email_outbox_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT
		id, 
		email_outbox_id, 
		file_size, 
		[file_name], 
		data
	FROM
		email_outbox_attachments
	WHERE
		email_outbox_id = @email_outbox_id
END
