-- =============================================
-- Author:		CRUD Creator
-- Create date: 06/10/2014 03:44:12 PM
-- Description:	Auto-generated method to insert a network_histories record.
-- =============================================
CREATE PROCEDURE usp_network_histories_insert
	@network_id INT,
	@start_date DATETIME,
	@code VARCHAR(15),
	@name VARCHAR(63),
	@active BIT,
	@flag TINYINT,
	@end_date DATETIME,
	@language_id TINYINT,
	@affiliated_network_id INT,
	@network_type_id TINYINT
AS
BEGIN
	INSERT INTO [dbo].[network_histories]
	(
		[network_id],
		[start_date],
		[code],
		[name],
		[active],
		[flag],
		[end_date],
		[language_id],
		[affiliated_network_id],
		[network_type_id]
	)
	VALUES
	(
		@network_id,
		@start_date,
		@code,
		@name,
		@active,
		@flag,
		@end_date,
		@language_id,
		@affiliated_network_id,
		@network_type_id
	)
END
