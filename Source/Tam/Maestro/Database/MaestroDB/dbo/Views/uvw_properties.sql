CREATE VIEW [dbo].[uvw_properties]
AS
SELECT     id AS property_id, name, value, effective_date AS start_date, NULL AS end_date
FROM         dbo.properties
UNION ALL
SELECT     property_id, name, value, start_date, end_date
FROM         dbo.property_histories
