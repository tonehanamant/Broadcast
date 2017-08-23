-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE usp_STS2_GetEmailOutboxStatusCode
	@email_outbox_id INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT
		dbo.GetEmailOutboxStatusCode(@email_outbox_id) 'email_outbox_status_code'
END
