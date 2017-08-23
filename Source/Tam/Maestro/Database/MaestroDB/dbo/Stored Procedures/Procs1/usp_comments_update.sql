
CREATE PROCEDURE [dbo].[usp_comments_update]
(
	@id		Int,
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
UPDATE comments SET
	comment_type_id = @comment_type_id,
	rtf_text = @rtf_text,
	plain_text = @plain_text,
	app_name = @app_name,
	form_name = @form_name,
	entity_name = @entity_name,
	reference_id = @reference_id,
	modified_date = @modified_date,
	employee_id = @employee_id
WHERE
	id = @id


