CREATE PROCEDURE [dbo].[usp_business_units_delete]
(
	@id TinyInt
)
AS
DELETE FROM business_units WHERE id=@id
