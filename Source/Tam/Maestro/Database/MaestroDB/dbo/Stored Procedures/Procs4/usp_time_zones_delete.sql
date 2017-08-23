
CREATE PROCEDURE [dbo].[usp_time_zones_delete]
(
	@id Int
,
	@effective_date DateTime
)
AS
UPDATE time_zones SET active=0, effective_date=@effective_date WHERE id=@id

