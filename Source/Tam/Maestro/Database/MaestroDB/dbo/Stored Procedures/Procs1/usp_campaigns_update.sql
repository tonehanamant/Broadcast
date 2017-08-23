
	CREATE PROCEDURE usp_campaigns_update
		@id INT,
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
		UPDATE
			[dbo].[campaigns]
		SET
			[name]=@name,
			[company_id]=@company_id,
			[date_created]=@date_created,
			[date_last_modified]=@date_last_modified,
			[budget]=@budget,
			[agency_id]=@agency_id,
			[version]=@version,
			[last_modified_by]=@last_modified_by,
			[start_date]=@start_date,
			[end_date]=@end_date
		WHERE
			[id]=@id
	END

