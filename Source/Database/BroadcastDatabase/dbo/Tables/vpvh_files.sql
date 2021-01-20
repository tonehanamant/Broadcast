CREATE TABLE [dbo].[vpvh_files] (
    [id]            INT           IDENTITY (1, 1) NOT NULL,
    [created_date]  DATETIME      NOT NULL,
    [created_by]    VARCHAR (63)  NOT NULL,
    [file_hash]     VARCHAR (255) NOT NULL,
    [file_name]     VARCHAR (255) NOT NULL,
    [success]       BIT           NOT NULL,
    [error_message] VARCHAR (MAX) NULL,
    CONSTRAINT [PK_vpvh_files] PRIMARY KEY CLUSTERED ([id] ASC)
);

