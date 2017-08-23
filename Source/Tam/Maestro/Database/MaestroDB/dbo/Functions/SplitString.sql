
-- =============================================
-- Author:		Rich Sara
-- Create date: 8/22/2010
-- Description:	Used to split variables into tables
--				(Part of SSRS Parms Integration)
-- =============================================

--IF EXISTS (SELECT * FROM sysobjects WHERE type in (N'FN', N'IF', N'TF', N'FS', N'FT') AND category = 0 AND 
--       name = 'SplitString')
--   DROP  Function  SplitString
--GO

CREATE Function [dbo].[SplitString](@StringToSplit varchar(max), @Delimiter varchar(50))
Returns @SplitTable Table
(
StringPart varchar(max)
)
As
Begin
Declare @StringRemaining varchar(max)
Set @StringRemaining = @StringToSplit
Declare @CharIndex int
Set @CharIndex = CharIndex(@Delimiter, @StringRemaining)
Declare @TableEntry varchar(max)
While @CharIndex > 0
Begin
Set @TableEntry = SubString(@StringRemaining, 1, @CharIndex-1)
Set @StringRemaining = SubString(@StringRemaining, @CharIndex+Len(@Delimiter), Len(@StringRemaining))
Insert Into @SplitTable (StringPart)
Values (@TableEntry)
Set @CharIndex = CharIndex(@Delimiter, @StringRemaining)
End
Insert Into @SplitTable (StringPart)
Values (@StringRemaining)
Return
End

