-- =============================================
-- Author:		Stephen DeFusco
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION GetEmailOutboxStatusCode
(
	@email_outbox_id INT
)
RETURNS TINYINT
AS
BEGIN
	DECLARE @return AS TINYINT

	DECLARE @total_details INT
	DECLARE @pending_details INT
	DECLARE @sent_details INT
	DECLARE @error_details INT

	SET @total_details =	(SELECT COUNT(*) FROM email_outbox_details (NOLOCK) WHERE email_outbox_id=@email_outbox_id);
	SET @sent_details =		(SELECT COUNT(*) FROM email_outbox_details (NOLOCK) WHERE email_outbox_id=@email_outbox_id AND status_code=1);
	SET @error_details =	(SELECT COUNT(*) FROM email_outbox_details (NOLOCK) WHERE email_outbox_id=@email_outbox_id AND status_code=2);
	SET @pending_details =	(SELECT COUNT(*) FROM email_outbox_details (NOLOCK) WHERE email_outbox_id=@email_outbox_id AND status_code=3);

	IF @total_details = @error_details
		SET @return = 4
	ELSE IF @total_details = @sent_details
		SET @return = 1
	ELSE IF @pending_details > 0 AND @sent_details > 0
		SET @return = 2
	ELSE IF @error_details > 0 AND @sent_details > 0
		SET @return = 3
	ELSE
		SET @return = 0
		
	RETURN @return
END
