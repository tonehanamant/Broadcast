CREATE PROCEDURE usp_employees_insert
(
	@id		Int		OUTPUT,
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
INSERT INTO employees
(
	username,
	accountdomainsid,
	firstname,
	lastname,
	mi,
	email,
	phone,
	internal_extension,
	status,
	datecreated,
	datelastlogin,
	datelastmodified,
	hitcount
)
VALUES
(
	@username,
	@accountdomainsid,
	@firstname,
	@lastname,
	@mi,
	@email,
	@phone,
	@internal_extension,
	@status,
	@datecreated,
	@datelastlogin,
	@datelastmodified,
	@hitcount
)

SELECT
	@id = SCOPE_IDENTITY()

