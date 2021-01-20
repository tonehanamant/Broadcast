CREATE TABLE [dbo].[inventory_file_problems] (
    [id]                  INT            IDENTITY (1, 1) NOT NULL,
    [inventory_file_id]   INT            NOT NULL,
    [problem_description] NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_inventory_file_problems] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_inventory_file_problems_inventory_files] FOREIGN KEY ([inventory_file_id]) REFERENCES [dbo].[inventory_files] ([id])
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_inventory_file_problems_inventory_files]
    ON [dbo].[inventory_file_problems]([inventory_file_id] ASC);

