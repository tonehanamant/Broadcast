CREATE PROCEDURE usp_states_delete
(
	@id Int
,
	@effective_date DateTime
)
AS
UPDATE states SET active=0, effective_date=@effective_date WHERE id=@id
