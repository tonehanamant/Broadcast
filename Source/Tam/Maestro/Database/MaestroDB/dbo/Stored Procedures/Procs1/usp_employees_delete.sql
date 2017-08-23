CREATE PROCEDURE usp_employees_delete
(
	@id Int
)
AS
DELETE FROM employees WHERE id=@id
