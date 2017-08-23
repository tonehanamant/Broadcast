-- =============================================
-- Author:		CRUD Creator
-- Create date: 06/10/2014 02:54:00 PM
-- Description:	Auto-generated method to insert a networks record.
-- =============================================
CREATE PROCEDURE usp_networks_insert
	@id INT OUTPUT,
	@code VARCHAR(15),
	@name VARCHAR(63),
	@active BIT,
	@flag TINYINT,
	@effective_date DATETIME,
	@language_id TINYINT,
	@affiliated_network_id INT,
	@network_type_id TINYINT
AS
BEGIN
	INSERT INTO [dbo].[networks]
	(
		[code],
		[name],
		[active],
		[flag],
		[effective_date],
		[language_id],
		[affiliated_network_id],
		[network_type_id]
	)
	VALUES
	(
		@code,
		@name,
		@active,
		@flag,
		@effective_date,
		@language_id,
		@affiliated_network_id,
		@network_type_id
	)

	SELECT
		@id = SCOPE_IDENTITY()
END
