
CREATE PROCEDURE usp_BS_InsertTrafficDetailStateLog
(
	@broadcast_traffic_detail_id int,
	@accepted bit,
	@employee_id int
)
AS

insert into broadcast_traffic_detail_statelog(broadcast_traffic_detail_id, effective_date, accepted, employee_id)
values (@broadcast_traffic_detail_id, getdate(), @accepted, @employee_id);

