CREATE TABLE [dbo].[nti_transmittals_file_problems] (
    [id]                       INT            IDENTITY (1, 1) NOT NULL,
    [nti_transmittals_file_id] INT            NOT NULL,
    [problem_description]      NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_nti_transmittals_file_problems] PRIMARY KEY CLUSTERED ([id] ASC),
    CONSTRAINT [FK_nti_transmittals_file_problems_nti_transmittals_files] FOREIGN KEY ([nti_transmittals_file_id]) REFERENCES [dbo].[nti_transmittals_files] ([id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_FK_nti_transmittals_file_problems_nti_transmittals_files]
    ON [dbo].[nti_transmittals_file_problems]([nti_transmittals_file_id] ASC);

