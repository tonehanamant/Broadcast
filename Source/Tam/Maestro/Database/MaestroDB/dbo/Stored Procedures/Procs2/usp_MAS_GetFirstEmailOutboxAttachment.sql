-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_MAS_GetFirstEmailOutboxAttachment
	@email_outbox_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT TOP 1
		id,
		email_outbox_id,
		file_size,
		[file_name],
		data
	FROM
		email_outbox_attachments (NOLOCK)
	WHERE
		email_outbox_id = @email_outbox_id
END
