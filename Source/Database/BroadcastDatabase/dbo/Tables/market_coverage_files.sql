CREATE TABLE [dbo].[market_coverage_files] (
    [id]           INT           IDENTITY (1, 1) NOT NULL,
    [file_name]    VARCHAR (255) NOT NULL,
    [file_hash]    VARCHAR (255) NOT NULL,
    [created_date] DATETIME      NOT NULL,
    [created_by]   VARCHAR (63)  NOT NULL,
    CONSTRAINT [PK_market_coverage_files] PRIMARY KEY CLUSTERED ([id] ASC)
);

