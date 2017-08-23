CREATE Procedure [dbo].[usp_REL_InsertAuditLog]
      (
            @traffic_id Int,
                                                @release_id int,
                                                @employee_id int,
                                                @message varchar(max)
      )
AS

INSERT INTO release_audit(release_id, traffic_id, message, event_date, employee_id)
VALUES (@release_id, @traffic_id, @message, getdate(), @employee_id); 