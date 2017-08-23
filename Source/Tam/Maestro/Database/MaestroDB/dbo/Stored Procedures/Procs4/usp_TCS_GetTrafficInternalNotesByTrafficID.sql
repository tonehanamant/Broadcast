


CREATE Procedure [dbo].[usp_TCS_GetTrafficInternalNotesByTrafficID]
(
     @traffic_id Int
)

AS

SELECT notes.id, notes.note_type, notes.reference_id, notes.employee_id, notes.comment, notes.date_created, notes.date_last_modified
 from notes (NOLOCK)
where notes.reference_id = @traffic_id and note_type = 'traffic'

 




