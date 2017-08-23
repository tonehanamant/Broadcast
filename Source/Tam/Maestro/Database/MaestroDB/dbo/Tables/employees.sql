CREATE TABLE [dbo].[employees] (
    [id]                 INT           IDENTITY (1, 1) NOT NULL,
    [username]           VARCHAR (50)  NOT NULL,
    [accountdomainsid]   VARCHAR (63)  NOT NULL,
    [firstname]          VARCHAR (50)  NOT NULL,
    [lastname]           VARCHAR (50)  NOT NULL,
    [mi]                 VARCHAR (1)   NOT NULL,
    [email]              VARCHAR (255) NOT NULL,
    [phone]              VARCHAR (15)  NULL,
    [internal_extension] VARCHAR (4)   NULL,
    [status]             TINYINT       NOT NULL,
    [datecreated]        DATETIME      CONSTRAINT [DF_employees_datecreated] DEFAULT (getdate()) NOT NULL,
    [datelastlogin]      DATETIME      CONSTRAINT [DF_employees_datelastlogin] DEFAULT (getdate()) NOT NULL,
    [datelastmodified]   DATETIME      CONSTRAINT [DF_employees_datelastmodified] DEFAULT (getdate()) NOT NULL,
    [hitcount]           INT           NOT NULL,
    CONSTRAINT [PK_employees] PRIMARY KEY CLUSTERED ([id] ASC) WITH (FILLFACTOR = 90)
);


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'employees';


GO
EXECUTE sp_addextendedproperty @name = N'FullName', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'employees';


GO
EXECUTE sp_addextendedproperty @name = N'Usage', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'employees';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'employees', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'employees', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'employees', @level2type = N'COLUMN', @level2name = N'username';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'employees', @level2type = N'COLUMN', @level2name = N'username';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'employees', @level2type = N'COLUMN', @level2name = N'accountdomainsid';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'employees', @level2type = N'COLUMN', @level2name = N'accountdomainsid';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'employees', @level2type = N'COLUMN', @level2name = N'firstname';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'employees', @level2type = N'COLUMN', @level2name = N'firstname';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'employees', @level2type = N'COLUMN', @level2name = N'lastname';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'employees', @level2type = N'COLUMN', @level2name = N'lastname';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'employees', @level2type = N'COLUMN', @level2name = N'mi';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'employees', @level2type = N'COLUMN', @level2name = N'mi';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'employees', @level2type = N'COLUMN', @level2name = N'email';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'employees', @level2type = N'COLUMN', @level2name = N'email';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'employees', @level2type = N'COLUMN', @level2name = N'phone';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'employees', @level2type = N'COLUMN', @level2name = N'phone';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'employees', @level2type = N'COLUMN', @level2name = N'internal_extension';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'employees', @level2type = N'COLUMN', @level2name = N'internal_extension';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'Value 0 = Enabled
Value 1 = Disabled', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'employees', @level2type = N'COLUMN', @level2name = N'status';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'employees', @level2type = N'COLUMN', @level2name = N'status';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'employees', @level2type = N'COLUMN', @level2name = N'datecreated';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'employees', @level2type = N'COLUMN', @level2name = N'datecreated';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'employees', @level2type = N'COLUMN', @level2name = N'datelastlogin';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'employees', @level2type = N'COLUMN', @level2name = N'datelastlogin';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'employees', @level2type = N'COLUMN', @level2name = N'datelastmodified';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'employees', @level2type = N'COLUMN', @level2name = N'datelastmodified';


GO
EXECUTE sp_addextendedproperty @name = N'Description', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'employees', @level2type = N'COLUMN', @level2name = N'hitcount';


GO
EXECUTE sp_addextendedproperty @name = N'Notes', @value = N'', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'employees', @level2type = N'COLUMN', @level2name = N'hitcount';

