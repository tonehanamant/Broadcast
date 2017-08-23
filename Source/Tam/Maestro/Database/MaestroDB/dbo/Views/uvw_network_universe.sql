CREATE VIEW [dbo].[uvw_network_universe]
AS
SELECT     id AS network_id, code, name, active, flag, effective_date AS start_date, NULL AS end_date, language_id, affiliated_network_id, network_type_id
FROM         dbo.networks (NOLOCK)
UNION ALL
SELECT     network_id, code, name, active, flag, start_date AS Expr1, end_date, language_id, affiliated_network_id, network_type_id
FROM         dbo.network_histories (NOLOCK)
