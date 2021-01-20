CREATE TYPE [dbo].[ProgramMappingUpdateRequests] AS TABLE (
    [program_name_mapping_id] INT            NOT NULL,
    [official_program_name]   NVARCHAR (500) NOT NULL,
    [genre_id]                INT            NOT NULL,
    [show_type_id]            INT            NOT NULL,
    PRIMARY KEY CLUSTERED ([program_name_mapping_id] ASC));

