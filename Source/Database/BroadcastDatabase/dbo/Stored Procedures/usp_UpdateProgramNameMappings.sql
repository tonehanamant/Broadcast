
CREATE PROCEDURE [dbo].[usp_UpdateProgramNameMappings]
	@update_requests ProgramMappingUpdateRequests READONLY,
    @modified_by varchar(63),
	@modified_at datetime
AS

/*
DECLARE
	@modified_by varchar(63)='db_queries_tester1',
	@modified_at datetime='2020-6-20',
	@update_requests ProgramMappingUpdateRequests

INSERT INTO @update_requests SELECT 49,'Program A v3',15,10
INSERT INTO @update_requests SELECT 51,'Program B v3',12,10
INSERT INTO @update_requests SELECT 52,'Program C v2',14,5

EXEC [dbo].[usp_UpdateProgramNameMappings] @update_requests, @modified_by, @modified_at
*/

BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON

	BEGIN TRAN
	BEGIN TRY
        update mapping
		set 
		mapping.official_program_name = request.official_program_name,
		mapping.genre_id = request.genre_id,
		mapping.show_type_id = request.show_type_id,
		mapping.modified_at = @modified_at,
		mapping.modified_by = @modified_by
		from program_name_mappings as mapping
		join @update_requests as request on mapping.id = request.program_name_mapping_id
        COMMIT TRAN
    END TRY
    BEGIN CATCH
        IF (@@TRANCOUNT > 0)
		BEGIN
		  ROLLBACK TRAN;
		END;

		THROW
    END CATCH
END