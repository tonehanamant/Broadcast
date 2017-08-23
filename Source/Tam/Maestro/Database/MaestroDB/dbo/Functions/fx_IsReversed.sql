CREATE FUNCTION [dbo].[fx_IsReversed]
(
@lCode varchar(20),
@lIsci varchar(20)
)
RETURNS bit
AS
BEGIN
declare @lLenCode int,
@lLenIsci int,
@lPosCode int,
@lPosIsci int
set @lLenCode= len(@lCode)
set @lLenIsci= len(@lIsci)
if (@lLenCode<>@lLenIsci)
Begin
return 0
End
set @lPosCode = 1
set @lPosIsci = @lLenIsci
declare @lCharCode char(1),
@lCharIsci char(1)
while (@lPosCode<=@lLenCode and @lPosIsci>=0)
Begin
set @lCharCode= substring(@lCode, @lPosCode, 1)
set @lCharIsci= substring(@lIsci, @lPosIsci, 1)
if (@lCharCode<>@lCharIsci)
Begin
return 0
End
set @lPosCode = @lPosCode + 1
set @lPosIsci = @lPosIsci - 1
End
Return 1
END
