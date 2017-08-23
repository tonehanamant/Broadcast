
-- Update the Campaign stored procs.
	CREATE PROCEDURE usp_campaigns_insert
		@id INT OUTPUT,
		@name NVARCHAR(127),
		@company_id INT,
		@date_created DATETIME,
		@date_last_modified DATETIME,
		@budget MONEY,
		@agency_id INT,
		@version INT,
		@last_modified_by INT,
		@start_date DATETIME,
		@end_date DATETIME
	AS
	BEGIN
		INSERT INTO [dbo].[campaigns]
		(
			[name],
			[company_id],
			[date_created],
			[date_last_modified],
			[budget],
			[agency_id],
			[version],
			[last_modified_by],
			[start_date],
			[end_date]
		)
		VALUES
		(
			@name,
			@company_id,
			@date_created,
			@date_last_modified,
			@budget,
			@agency_id,
			@version,
			@last_modified_by,
			@start_date,
			@end_date
		)
	
		SELECT
			@id = SCOPE_IDENTITY()
	END

