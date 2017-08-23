-- =============================================
-- Author:		CRUD Creator
-- Create date: 10/15/2013 12:40:24 PM
-- Description:	Auto-generated method to insert a roles record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_roles_insert]
	@id INT OUTPUT,
	@name VARCHAR(63),
	@description VARCHAR(3071),
	@maestro_composers VARCHAR(63)
AS
BEGIN
	INSERT INTO [dbo].[roles]
	(
		[name],
		[description],
		[maestro_composers]
	)
	VALUES
	(
		@name,
		@description,
		@maestro_composers
	)

	SELECT
		@id = SCOPE_IDENTITY()
END
