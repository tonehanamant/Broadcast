-- =============================================
-- Author:		CRUD Creator
-- Create date: 06/30/2015 02:31:00 PM
-- Description:	Auto-generated method to insert a system_statements record.
-- =============================================
CREATE PROCEDURE [dbo].[usp_system_statements_insert]
	@id INT OUTPUT,
	@statement_id INT,
	@system_statement_group_id INT,
	@system_id INT,
	@name VARCHAR(63),
	@status TINYINT,
	@enabled BIT,
	@confirmed_amount MONEY
AS
BEGIN
	INSERT INTO [dbo].[system_statements]
	(
		[statement_id],
		[system_statement_group_id],
		[system_id],
		[name],
		[status],
		[enabled],
		[confirmed_amount]
	)
	VALUES
	(
		@statement_id,
		@system_statement_group_id,
		@system_id,
		@name,
		@status,
		@enabled,
		@confirmed_amount
	)

	SELECT
		@id = SCOPE_IDENTITY()
END

