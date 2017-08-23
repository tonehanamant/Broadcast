
---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
-- =============================================
-- Author:		CRUD Creator
-- Create date: 04/07/2015 12:54:01 PM
-- Description:	Auto-generated method to insert a maestro_global_audit_trail record.
-- =============================================
CREATE PROCEDURE usp_maestro_global_audit_trail_insert
	@id BIGINT OUTPUT,
	@entity_id VARCHAR(100),
	@entity_name VARCHAR(255),
	@username VARCHAR(127),
	@time_stamp DATETIME,
	@audit_type TINYINT,
	@property_name VARCHAR(255),
	@old_value VARCHAR(511),
	@new_value VARCHAR(511)
AS
BEGIN
	INSERT INTO [dbo].[maestro_global_audit_trail]
	(
		[entity_id],
		[entity_name],
		[username],
		[time_stamp],
		[audit_type],
		[property_name],
		[old_value],
		[new_value]
	)
	VALUES
	(
		@entity_id,
		@entity_name,
		@username,
		@time_stamp,
		@audit_type,
		@property_name,
		@old_value,
		@new_value
	)

	SELECT
		@id = SCOPE_IDENTITY()
END
