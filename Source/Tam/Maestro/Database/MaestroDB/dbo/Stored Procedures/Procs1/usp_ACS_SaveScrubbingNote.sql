-- =============================================
-- Author:		Stephen DeFusco
-- Create date: 1/18/2011
-- Description:	Returns a scrubbing note matching the specified criteria.
-- =============================================
CREATE PROCEDURE [dbo].[usp_ACS_SaveScrubbingNote]
	@scrubber_note_id INT,
	@media_month_id INT,
	@scrubber_error_code TINYINT,
	@invalid_item VARCHAR(2047),
	@comment VARCHAR(4096),
	@id INT OUTPUT
AS
BEGIN
	IF @scrubber_note_id IS NOT NULL
		BEGIN	
			UPDATE scrubber_notes SET comment=@comment, date_last_modified=GETDATE() WHERE id=@scrubber_note_id
			SET @id = @scrubber_note_id
		END
	ELSE
		BEGIN
			INSERT INTO scrubber_notes (media_month_id, scrubber_error_code, invalid_item, comment, date_created, date_last_modified)
				VALUES (@media_month_id, @scrubber_error_code, @invalid_item, @comment, GETDATE(), GETDATE())
			
			SET @id = SCOPE_IDENTITY()
		END

	SELECT
		@id
END
