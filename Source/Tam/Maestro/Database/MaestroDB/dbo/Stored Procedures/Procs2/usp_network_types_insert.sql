-- =============================================
-- Author:		CRUD Creator
-- Create date: 06/10/2014 02:12:31 PM
-- Description:	Auto-generated method to insert a network_types record.
-- =============================================
CREATE PROCEDURE usp_network_types_insert
	@id TINYINT OUTPUT,
	@name VARCHAR(31)
AS
BEGIN
	INSERT INTO [dbo].[network_types]
	(
		[name]
	)
	VALUES
	(
		@name
	)

	SELECT
		@id = SCOPE_IDENTITY()
END
