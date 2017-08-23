

 

 

CREATE Procedure [dbo].[usp_TCS_GetTrafficExternalNotesByTrafficID]

(

     @traffic_id Int

)

 

AS

 

SELECT notes.id, notes.note_type, notes.reference_id, notes.employee_id, notes.comment, notes.date_created, notes.date_last_modified

 from notes (NOLOCK), traffic  (NOLOCK)

where notes.id = traffic.external_note_id and traffic.id = @traffic_id

 


