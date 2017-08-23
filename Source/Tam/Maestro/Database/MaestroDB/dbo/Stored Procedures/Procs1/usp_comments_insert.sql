
CREATE PROCEDURE [dbo].[usp_comments_insert]
(
	@id		Int		OUTPUT,
	@comment_type_id		Int,
	@rtf_text		VARCHAR(MAX),
	@plain_text		VARCHAR(MAX),
	@app_name		VarChar(100),
	@form_name		VarChar(100),
	@entity_name		VarChar(100),
	@reference_id		Int,
	@modified_date		DateTime,
	@employee_id		Int
)
AS
INSERT INTO comments
(
	comment_type_id,
	rtf_text,
	plain_text,
	app_name,
	form_name,
	entity_name,
	reference_id,
	modified_date,
	employee_id
)
VALUES
(
	@comment_type_id,
	@rtf_text,
	@plain_text,
	@app_name,
	@form_name,
	@entity_name,
	@reference_id,
	@modified_date,
	@employee_id
)

SELECT
	@id = SCOPE_IDENTITY()


