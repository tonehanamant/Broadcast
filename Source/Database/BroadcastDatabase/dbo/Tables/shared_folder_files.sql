CREATE TABLE [dbo].[shared_folder_files] (
    [id]              UNIQUEIDENTIFIER NOT NULL,
    [folder_path]     VARCHAR (MAX)    NOT NULL,
    [file_name]       VARCHAR (255)    NOT NULL,
    [file_extension]  VARCHAR (15)     NOT NULL,
    [file_media_type] VARCHAR (255)    NOT NULL,
    [file_usage]      INT              NOT NULL,
    [created_date]    DATETIME         NOT NULL,
    [created_by]      VARCHAR (63)     NOT NULL,
    CONSTRAINT [PK_shared_folder_files] PRIMARY KEY CLUSTERED ([id] ASC)
);

