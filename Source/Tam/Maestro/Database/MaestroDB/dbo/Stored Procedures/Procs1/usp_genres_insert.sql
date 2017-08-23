-- =============================================
-- Author:		CRUD Creator
-- Create date: 06/11/2014 12:41:07 PM
-- Description:	Auto-generated method to insert a genres record.
-- =============================================
CREATE PROCEDURE usp_genres_insert
	@id INT,
	@name VARCHAR(63)
AS
BEGIN
	INSERT INTO [dbo].[genres]
	(
		[id],
		[name]
	)
	VALUES
	(
		@id,
		@name
	)
END
