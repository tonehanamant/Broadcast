CREATE TABLE [dbo].[fluidity_categories] (
    [id]           INT            IDENTITY (1, 1) NOT NULL,
    [code] VARCHAR(50) NULL,
    [category] VARCHAR(50) NOT NULL, 
    [parent_category_id] INT NULL, 
    CONSTRAINT [PK_fluidity_categories] PRIMARY KEY CLUSTERED ([id] ASC)
);

