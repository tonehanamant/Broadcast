
CREATE PROCEDURE [dbo].[usp_salesforce_synchronization_insert]
(
	@id		Int		OUTPUT,
	@synchronization_type		VarChar(50),
	@last_sync_date		DateTime,
	@modified_date		DateTime,
	@modified_by		VarChar(50)
)
AS
INSERT INTO salesforce_synchronization
(
	synchronization_type,
	last_sync_date,
	modified_date,
	modified_by
)
VALUES
(
	@synchronization_type,
	@last_sync_date,
	@modified_date,
	@modified_by
)

SELECT
	@id = SCOPE_IDENTITY()


