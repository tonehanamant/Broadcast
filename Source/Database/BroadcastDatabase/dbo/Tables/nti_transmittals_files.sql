CREATE TABLE [dbo].[nti_transmittals_files] (
    [id]           INT           IDENTITY (1, 1) NOT NULL,
    [file_name]    VARCHAR (255) NOT NULL,
    [created_date] DATETIME      NOT NULL,
    [created_by]   VARCHAR (63)  NOT NULL,
    [status]       INT           NOT NULL,
    CONSTRAINT [PK_nti_transmittals_files] PRIMARY KEY CLUSTERED ([id] ASC)
);

