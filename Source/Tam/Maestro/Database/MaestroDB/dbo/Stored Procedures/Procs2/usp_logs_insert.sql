CREATE PROCEDURE usp_logs_insert
(
	@id		Int		OUTPUT,
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
INSERT INTO logs
(
	employee_id,
	event_type,
	event_code,
	event_timestamp,
	source,
	host_name,
	description,
	exception_message,
	exception_stack_trace,
	exception_source
)
VALUES
(
	@employee_id,
	@event_type,
	@event_code,
	@event_timestamp,
	@source,
	@host_name,
	@description,
	@exception_message,
	@exception_stack_trace,
	@exception_source
)

SELECT
	@id = SCOPE_IDENTITY()

