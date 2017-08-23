CREATE PROCEDURE usp_employees_update
(
	@id		Int,
	@username		VarChar(50),
	@accountdomainsid		VarChar(63),
	@firstname		VarChar(50),
	@lastname		VarChar(50),
	@mi		VarChar(1),
	@email		VarChar(255),
	@phone		VarChar(15),
	@internal_extension		VarChar(4),
	@status		TinyInt,
	@datecreated		DateTime,
	@datelastlogin		DateTime,
	@datelastmodified		DateTime,
	@hitcount		Int
)
AS
UPDATE employees SET
	username = @username,
	accountdomainsid = @accountdomainsid,
	firstname = @firstname,
	lastname = @lastname,
	mi = @mi,
	email = @email,
	phone = @phone,
	internal_extension = @internal_extension,
	status = @status,
	datecreated = @datecreated,
	datelastlogin = @datelastlogin,
	datelastmodified = @datelastmodified,
	hitcount = @hitcount
WHERE
	id = @id

