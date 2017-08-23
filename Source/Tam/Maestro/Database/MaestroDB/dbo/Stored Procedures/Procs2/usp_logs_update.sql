CREATE PROCEDURE usp_logs_update
(
	@id		Int,
	@employee_id		Int,
	@event_type		TinyInt,
	@event_code		Int,
	@event_timestamp		DateTime,
	@source		TinyInt,
	@host_name		VarChar(255),
	@description		Text,
	@exception_message		Text,
	@exception_stack_trace		Text,
	@exception_source		Text
)
AS
UPDATE logs SET
	employee_id = @employee_id,
	event_type = @event_type,
	event_code = @event_code,
	event_timestamp = @event_timestamp,
	source = @source,
	host_name = @host_name,
	description = @description,
	exception_message = @exception_message,
	exception_stack_trace = @exception_stack_trace,
	exception_source = @exception_source
WHERE
	id = @id

