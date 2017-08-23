CREATE PROCEDURE [dbo].[usp_autoloaded_rating_categories_insert]
	@rating_category_id INT,
	@watch_folder VARCHAR(100),
	@filename_template VARCHAR(100),
	@watch_frequency_id INT
AS
BEGIN
	INSERT INTO [dbo].[autoloaded_rating_categories]
	(
		[rating_category_id],
		[watch_folder],
		[filename_template],
		[watch_frequency_id]
	)
	VALUES
	(
		@rating_category_id,
		@watch_folder,
		@filename_template,
		@watch_frequency_id
	)
END
