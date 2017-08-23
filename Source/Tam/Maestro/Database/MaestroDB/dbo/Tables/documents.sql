CREATE TABLE [dbo].[documents] (
    [id]        INT             IDENTITY (1, 1) NOT NULL,
    [file_name] VARCHAR (255)   NOT NULL,
    [file_size] BIGINT          NOT NULL,
    [hash]      VARCHAR (63)    NOT NULL,
    [file_data] VARBINARY (MAX) NOT NULL,
    CONSTRAINT [PK_documents] PRIMARY KEY CLUSTERED ([id] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Contains physical files.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'documents';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'Documents', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'documents';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'Used a central location for storing files.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'documents';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Unique Identifier.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'documents', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'documents', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The name of the file with extension.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'documents', @level2type = N'COLUMN', @level2name = N'file_name';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'documents', @level2type = N'COLUMN', @level2name = N'file_name';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The physical size of the file in bytes.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'documents', @level2type = N'COLUMN', @level2name = N'file_size';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'documents', @level2type = N'COLUMN', @level2name = N'file_size';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The SHA1 calculated hash code of the file.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'documents', @level2type = N'COLUMN', @level2name = N'hash';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'Useful for detecting if one file is exactly the same as another.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'documents', @level2type = N'COLUMN', @level2name = N'hash';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'The binary data of the file.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'documents', @level2type = N'COLUMN', @level2name = N'file_data';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'documents', @level2type = N'COLUMN', @level2name = N'file_data';

