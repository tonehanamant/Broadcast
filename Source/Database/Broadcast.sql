---------------------------------------------------------------------------------------------------
-- !!! If Executing from SQL Server Manager, please enable SQLCMD Mode!!! To enable option, select menu Query->Enable SQLCMD mode. --
---------------------------------------------------------------------------------------------------
-- All scripts should be written in a way that they can be run multiple times
-- All features/bugs should be wrapped in comments indicating the start/end of the scripts
---------------------------------------------------------------------------------------------------
---------------------------------------------------------------------------------------------------
-- TFS Items:
---------------------------------------------------------------------------------------------------
---------------------------------------------------------------------------------------------------
SET NOEXEC OFF;
SET NOCOUNT OFF;
GO

:on error exit --sqlcmd exit script on error
GO
:setvar __IsSqlCmdEnabled "True"
GO
IF N'$(__IsSqlCmdEnabled)' NOT LIKE N'True'
    BEGIN
        PRINT N'SQLCMD mode must be enabled to successfully execute this script.To enable option, select menu Query->Enable SQLCMD mode';
		SET NOCOUNT ON;
        SET NOEXEC ON; -- this will not execute any queries. queries will be compiled only.
    END
GO

SET XACT_ABORT ON -- Rollback transaction incase of error
GO



BEGIN
	PRINT 'RUNNING SCRIPT IN LOCAL DATBASE'
END
GO


BEGIN TRANSACTION

CREATE TABLE #previous_version 
( 
	[version] VARCHAR(32) 
)
GO

-- Only run this script when the schema is in the correct pervious version
INSERT INTO #previous_version
		SELECT parameter_value 
		FROM system_component_parameters 
		WHERE parameter_key = 'SchemaVersion' 


/*************************************** START UPDATE SCRIPT *****************************************************/



/*************************************** BCOP-1370 BEGIN ***************************************************/
GO
IF (NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'market_dma_map'))
BEGIN
    CREATE TABLE market_dma_map
	(
		[market_code] [smallint] NOT NULL,
		[dma_mapped_value] varchar(63) not null,
		constraint [pk_market_dma_map] primary key clustered (market_code)
	);
END

GO

if not exists (SELECT     *  FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS     WHERE CONSTRAINT_NAME ='FK_market_dma_map_market_code')
BEGIN
	ALTER TABLE [dbo].[market_dma_map]  WITH CHECK ADD  CONSTRAINT [FK_market_dma_map_market_code] FOREIGN KEY([market_code])
	REFERENCES [dbo].[markets] ([market_code])
END

GO


if not exists(select 1 from market_dma_map WHERE market_code = 100) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 100,'Portland, ME');
END
if not exists(select 1 from market_dma_map WHERE market_code = 101) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 101,'New York');
END
if not exists(select 1 from market_dma_map WHERE market_code = 102) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 102,'Binghamton');
END
if not exists(select 1 from market_dma_map WHERE market_code = 103) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 103,'Macon');
END
if not exists(select 1 from market_dma_map WHERE market_code = 104) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 104,'Philadelphia');
END
if not exists(select 1 from market_dma_map WHERE market_code = 105) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 105,'Detroit');
END
if not exists(select 1 from market_dma_map WHERE market_code = 106) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 106,'Boston');
END
if not exists(select 1 from market_dma_map WHERE market_code = 107) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 107,'Savannah');
END
if not exists(select 1 from market_dma_map WHERE market_code = 108) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 108,'Pittsburgh');
END
if not exists(select 1 from market_dma_map WHERE market_code = 109) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 109,'Ft. Wayne');
END
if not exists(select 1 from market_dma_map WHERE market_code = 110) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 110,'Cleveland');
END
if not exists(select 1 from market_dma_map WHERE market_code = 111) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 111,'Washington, DC');
END
if not exists(select 1 from market_dma_map WHERE market_code = 112) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 112,'Baltimore');
END
if not exists(select 1 from market_dma_map WHERE market_code = 113) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 113,'Flint');
END
if not exists(select 1 from market_dma_map WHERE market_code = 114) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 114,'Buffalo-Niagara Falls');
END
if not exists(select 1 from market_dma_map WHERE market_code = 115) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 115,'Cincinnati');
END
if not exists(select 1 from market_dma_map WHERE market_code = 116) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 116,'Erie');
END
if not exists(select 1 from market_dma_map WHERE market_code = 117) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 117,'Charlotte-Gastonia-Rk Hill');
END
if not exists(select 1 from market_dma_map WHERE market_code = 118) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 118,'Greensboro-Winston Salem');
END
if not exists(select 1 from market_dma_map WHERE market_code = 119) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 119,'Charleston, SC');
END
if not exists(select 1 from market_dma_map WHERE market_code = 120) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 120,'Augusta, GA');
END
if not exists(select 1 from market_dma_map WHERE market_code = 121) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 121,'Providence-Warwick');
END
if not exists(select 1 from market_dma_map WHERE market_code = 122) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 122,'Columbus, GA');
END
if not exists(select 1 from market_dma_map WHERE market_code = 123) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 123,'Burlington, VT');
END
if not exists(select 1 from market_dma_map WHERE market_code = 124) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 124,'Atlanta');
END
if not exists(select 1 from market_dma_map WHERE market_code = 125) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 125,'Albany, GA');
END
if not exists(select 1 from market_dma_map WHERE market_code = 126) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 126,'Utica-Rome');
END
if not exists(select 1 from market_dma_map WHERE market_code = 127) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 127,'Indianapolis');
END
if not exists(select 1 from market_dma_map WHERE market_code = 128) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 128,'Miami-Ft. Lauderdale');
END
if not exists(select 1 from market_dma_map WHERE market_code = 129) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 129,'Louisville');
END
if not exists(select 1 from market_dma_map WHERE market_code = 130) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 130,'Tallahassee');
END
if not exists(select 1 from market_dma_map WHERE market_code = 131) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 131,'Johnsn City-Kngsprt-Bristl');
END
if not exists(select 1 from market_dma_map WHERE market_code = 132) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 132,'Albany-Schenectady-Troy');
END
if not exists(select 1 from market_dma_map WHERE market_code = 133) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 133,'Hartford, CT');
END
if not exists(select 1 from market_dma_map WHERE market_code = 134) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 134,'Orlando');
END
if not exists(select 1 from market_dma_map WHERE market_code = 135) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 135,'Columbus, OH');
END
if not exists(select 1 from market_dma_map WHERE market_code = 136) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 136,'Youngstown-Warren');
END
if not exists(select 1 from market_dma_map WHERE market_code = 137) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 137,'Bangor');
END
if not exists(select 1 from market_dma_map WHERE market_code = 138) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 138,'Rochester, NY');
END
if not exists(select 1 from market_dma_map WHERE market_code = 139) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 139,'Tampa-St. Petersburg');
END
if not exists(select 1 from market_dma_map WHERE market_code = 140) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 140,'Traverse City');
END
if not exists(select 1 from market_dma_map WHERE market_code = 141) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 141,'Lexington-Fayette');
END
if not exists(select 1 from market_dma_map WHERE market_code = 142) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 142,'Dayton');
END
if not exists(select 1 from market_dma_map WHERE market_code = 143) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 143,'Springfield, MA');
END
if not exists(select 1 from market_dma_map WHERE market_code = 144) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 144,'Norfolk-Virginia Beach');
END
if not exists(select 1 from market_dma_map WHERE market_code = 145) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 145,'Greenville/New Bern/Wash');
END
if not exists(select 1 from market_dma_map WHERE market_code = 146) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 146,'Columbia, SC');
END
if not exists(select 1 from market_dma_map WHERE market_code = 147) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 147,'Toledo');
END
if not exists(select 1 from market_dma_map WHERE market_code = 148) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 148,'West Palm Beach-Boca Raton');
END
if not exists(select 1 from market_dma_map WHERE market_code = 149) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 149,'Watertown');
END
if not exists(select 1 from market_dma_map WHERE market_code = 150) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 150,'Wilmington, NC');
END
if not exists(select 1 from market_dma_map WHERE market_code = 151) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 151,'Lansing-East Lansing');
END
if not exists(select 1 from market_dma_map WHERE market_code = 152) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 152,'Presque Isle');
END
if not exists(select 1 from market_dma_map WHERE market_code = 153) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 153,'Marquette');
END
if not exists(select 1 from market_dma_map WHERE market_code = 154) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 154,'Wheeling');
END
if not exists(select 1 from market_dma_map WHERE market_code = 155) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 155,'Syracuse');
END
if not exists(select 1 from market_dma_map WHERE market_code = 156) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 156,'Richmond');
END
if not exists(select 1 from market_dma_map WHERE market_code = 157) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 157,'Knoxville');
END
if not exists(select 1 from market_dma_map WHERE market_code = 158) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 158,'Lima, OH');
END
if not exists(select 1 from market_dma_map WHERE market_code = 159) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 159,'Bluefield WV-VA');
END
if not exists(select 1 from market_dma_map WHERE market_code = 160) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 160,'Raleigh-Durham');
END
if not exists(select 1 from market_dma_map WHERE market_code = 161) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 161,'Jacksonville');
END
if not exists(select 1 from market_dma_map WHERE market_code = 163) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 163,'Grand Rapids');
END
if not exists(select 1 from market_dma_map WHERE market_code = 164) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 164,'Charleston, WV');
END
if not exists(select 1 from market_dma_map WHERE market_code = 165) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 165,'Elmira - Corning, NY');
END
if not exists(select 1 from market_dma_map WHERE market_code = 166) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 166,'Harrisburg-Lebanon');
END
if not exists(select 1 from market_dma_map WHERE market_code = 167) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 167,'Greenville-Spartanburg');
END
if not exists(select 1 from market_dma_map WHERE market_code = 169) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 169,'Harrisonburg, VA');
END
if not exists(select 1 from market_dma_map WHERE market_code = 170) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 170,'Florence, SC');
END
if not exists(select 1 from market_dma_map WHERE market_code = 171) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 171,'Ft. Myers-Naples-Marco Isl');
END
if not exists(select 1 from market_dma_map WHERE market_code = 173) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 173,'Roanoke-Lynchburg');
END
if not exists(select 1 from market_dma_map WHERE market_code = 174) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 174,'Johnstown, PA');
END
if not exists(select 1 from market_dma_map WHERE market_code = 175) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 175,'Chattanooga');
END
if not exists(select 1 from market_dma_map WHERE market_code = 176) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 176,'Salisbury-Ocean City');
END
if not exists(select 1 from market_dma_map WHERE market_code = 177) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 177,'Wilkes Barre-Scranton');
END
if not exists(select 1 from market_dma_map WHERE market_code = 181) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 181,'Terre Haute');
END
if not exists(select 1 from market_dma_map WHERE market_code = 182) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 182,'Lafayette, IN');
END
if not exists(select 1 from market_dma_map WHERE market_code = 183) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 183,'Alpena');
END
if not exists(select 1 from market_dma_map WHERE market_code = 184) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 184,'Charlottesville, VA');
END
if not exists(select 1 from market_dma_map WHERE market_code = 188) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 188,'South Bend');
END
if not exists(select 1 from market_dma_map WHERE market_code = 192) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 192,'Gainesville, FL');
END
if not exists(select 1 from market_dma_map WHERE market_code = 196) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 196,'Zanesville');
END
if not exists(select 1 from market_dma_map WHERE market_code = 197) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 197,'Parkersburg-Marietta');
END
if not exists(select 1 from market_dma_map WHERE market_code = 198) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 198,'Clarksburg');
END
if not exists(select 1 from market_dma_map WHERE market_code = 200) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 200,'Corpus Christi');
END
if not exists(select 1 from market_dma_map WHERE market_code = 202) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 202,'Chicago');
END
if not exists(select 1 from market_dma_map WHERE market_code = 203) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 203,'Joplin');
END
if not exists(select 1 from market_dma_map WHERE market_code = 204) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 204,'Columbia, MO');
END
if not exists(select 1 from market_dma_map WHERE market_code = 205) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 205,'Topeka');
END
if not exists(select 1 from market_dma_map WHERE market_code = 206) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 206,'Dothan');
END
if not exists(select 1 from market_dma_map WHERE market_code = 209) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 209,'St. Louis');
END
if not exists(select 1 from market_dma_map WHERE market_code = 210) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 210,'Rockford');
END
if not exists(select 1 from market_dma_map WHERE market_code = 211) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 211,'Rochester, MN');
END
if not exists(select 1 from market_dma_map WHERE market_code = 212) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 212,'Shreveport');
END
if not exists(select 1 from market_dma_map WHERE market_code = 213) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 213,'Minneapolis-St. Paul');
END
if not exists(select 1 from market_dma_map WHERE market_code = 216) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 216,'Kansas City');
END
if not exists(select 1 from market_dma_map WHERE market_code = 217) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 217,'Milwaukee-Racine');
END
if not exists(select 1 from market_dma_map WHERE market_code = 218) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 218,'Houston-Galveston');
END
if not exists(select 1 from market_dma_map WHERE market_code = 219) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 219,'Springfield, MO');
END
if not exists(select 1 from market_dma_map WHERE market_code = 222) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 222,'New Orleans');
END
if not exists(select 1 from market_dma_map WHERE market_code = 223) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 223,'Dallas-Ft. Worth');
END
if not exists(select 1 from market_dma_map WHERE market_code = 224) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 224,'Sioux City');
END
if not exists(select 1 from market_dma_map WHERE market_code = 225) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 225,'Waco');
END
if not exists(select 1 from market_dma_map WHERE market_code = 226) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 226,'Victoria');
END
if not exists(select 1 from market_dma_map WHERE market_code = 227) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 227,'Wichita Falls');
END
if not exists(select 1 from market_dma_map WHERE market_code = 228) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 228,'Monroe');
END
if not exists(select 1 from market_dma_map WHERE market_code = 230) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 230,'Birmingham');
END
if not exists(select 1 from market_dma_map WHERE market_code = 231) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 231,'Ottumwa');
END
if not exists(select 1 from market_dma_map WHERE market_code = 232) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 232,'Paducah');
END
if not exists(select 1 from market_dma_map WHERE market_code = 233) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 233,'Odessa-Midland');
END
if not exists(select 1 from market_dma_map WHERE market_code = 234) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 234,'Amarillo');
END
if not exists(select 1 from market_dma_map WHERE market_code = 235) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 235,'Austin');
END
if not exists(select 1 from market_dma_map WHERE market_code = 236) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 236,'Harlingen');
END
if not exists(select 1 from market_dma_map WHERE market_code = 237) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 237,'Cedar Rapids');
END
if not exists(select 1 from market_dma_map WHERE market_code = 238) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 238,'St. Joseph');
END
if not exists(select 1 from market_dma_map WHERE market_code = 239) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 239,'Jackson, TN');
END
if not exists(select 1 from market_dma_map WHERE market_code = 240) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 240,'Memphis');
END
if not exists(select 1 from market_dma_map WHERE market_code = 241) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 241,'San Antonio');
END
if not exists(select 1 from market_dma_map WHERE market_code = 242) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 242,'Lafayette, LA');
END
if not exists(select 1 from market_dma_map WHERE market_code = 243) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 243,'Lake Charles');
END
if not exists(select 1 from market_dma_map WHERE market_code = 244) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 244,'Alexandria');
END
if not exists(select 1 from market_dma_map WHERE market_code = 247) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 247,'Greenwood-Greenville');
END
if not exists(select 1 from market_dma_map WHERE market_code = 248) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 248,'Champaign-Sprngfld-Dtr DMA');
END
if not exists(select 1 from market_dma_map WHERE market_code = 249) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 249,'Evansville');
END
if not exists(select 1 from market_dma_map WHERE market_code = 250) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 250,'Oklahoma City');
END
if not exists(select 1 from market_dma_map WHERE market_code = 251) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 251,'Lubbock');
END
if not exists(select 1 from market_dma_map WHERE market_code = 252) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 252,'Omaha-Council Bluffs');
END
if not exists(select 1 from market_dma_map WHERE market_code = 256) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 256,'Panama City');
END
if not exists(select 1 from market_dma_map WHERE market_code = 257) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 257,'Sherman-Ada');
END
if not exists(select 1 from market_dma_map WHERE market_code = 258) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 258,'Green Bay');
END
if not exists(select 1 from market_dma_map WHERE market_code = 259) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 259,'Nashville');
END
if not exists(select 1 from market_dma_map WHERE market_code = 261) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 261,'San Angelo');
END
if not exists(select 1 from market_dma_map WHERE market_code = 262) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 262,'Abilene');
END
if not exists(select 1 from market_dma_map WHERE market_code = 269) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 269,'Madison');
END
if not exists(select 1 from market_dma_map WHERE market_code = 270) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 270,'Ft. Smith, AR');
END
if not exists(select 1 from market_dma_map WHERE market_code = 271) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 271,'Tulsa');
END
if not exists(select 1 from market_dma_map WHERE market_code = 273) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 273,'Columbus-Tupelo');
END
if not exists(select 1 from market_dma_map WHERE market_code = 275) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 275,'Peoria');
END
if not exists(select 1 from market_dma_map WHERE market_code = 276) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 276,'Duluth-Superior');
END
if not exists(select 1 from market_dma_map WHERE market_code = 278) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 278,'Wichita');
END
if not exists(select 1 from market_dma_map WHERE market_code = 279) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 279,'Des Moines');
END
if not exists(select 1 from market_dma_map WHERE market_code = 282) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 282,'Quad Cities (Dav-RI-Mol)');
END
if not exists(select 1 from market_dma_map WHERE market_code = 286) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 286,'Mobile');
END
if not exists(select 1 from market_dma_map WHERE market_code = 287) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 287,'Minot');
END
if not exists(select 1 from market_dma_map WHERE market_code = 291) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 291,'Huntsville');
END
if not exists(select 1 from market_dma_map WHERE market_code = 292) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 292,'Beaumont-Port Arthur');
END
if not exists(select 1 from market_dma_map WHERE market_code = 293) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 293,'Little Rock');
END
if not exists(select 1 from market_dma_map WHERE market_code = 298) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 298,'Montgomery');
END
if not exists(select 1 from market_dma_map WHERE market_code = 302) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 302,'La Crosse');
END
if not exists(select 1 from market_dma_map WHERE market_code = 305) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 305,'Wausau, WI');
END
if not exists(select 1 from market_dma_map WHERE market_code = 309) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 309,'Tyler');
END
if not exists(select 1 from market_dma_map WHERE market_code = 310) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 310,'Laurel-Hattiesburg, MS');
END
if not exists(select 1 from market_dma_map WHERE market_code = 311) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 311,'Meridian');
END
if not exists(select 1 from market_dma_map WHERE market_code = 316) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 316,'Baton Rouge');
END
if not exists(select 1 from market_dma_map WHERE market_code = 317) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 317,'Quincy');
END
if not exists(select 1 from market_dma_map WHERE market_code = 318) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 318,'Jackson, MS');
END
if not exists(select 1 from market_dma_map WHERE market_code = 322) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 322,'Lincoln&Hstng-Krny-Pls DMA');
END
if not exists(select 1 from market_dma_map WHERE market_code = 324) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 324,'Fargo-Moorhead');
END
if not exists(select 1 from market_dma_map WHERE market_code = 325) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 325,'Sioux Falls');
END
if not exists(select 1 from market_dma_map WHERE market_code = 334) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 334,'Jonesboro/Paragould, AR');
END
if not exists(select 1 from market_dma_map WHERE market_code = 336) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 336,'Bowling Green/Glasgow, KY');
END
if not exists(select 1 from market_dma_map WHERE market_code = 337) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 337,'Mankato- St. Peter, MN');
END
if not exists(select 1 from market_dma_map WHERE market_code = 340) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 340,'North Platte/Ogallala, NE');
END
if not exists(select 1 from market_dma_map WHERE market_code = 343) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 343,'Anchorage');
END
if not exists(select 1 from market_dma_map WHERE market_code = 344) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 344,'Honolulu');
END
if not exists(select 1 from market_dma_map WHERE market_code = 345) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 345,'Fairbanks, AK');
END
if not exists(select 1 from market_dma_map WHERE market_code = 346) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 346,'Biloxi-Gulfport');
END
if not exists(select 1 from market_dma_map WHERE market_code = 347) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 347,'Juneau, AK');
END
if not exists(select 1 from market_dma_map WHERE market_code = 349) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 349,'Laredo, TX');
END
if not exists(select 1 from market_dma_map WHERE market_code = 351) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 351,'Denver-Boulder');
END
if not exists(select 1 from market_dma_map WHERE market_code = 352) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 352,'Colorado Springs');
END
if not exists(select 1 from market_dma_map WHERE market_code = 353) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 353,'Phoenix');
END
if not exists(select 1 from market_dma_map WHERE market_code = 354) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 354,'Butte, MT');
END
if not exists(select 1 from market_dma_map WHERE market_code = 355) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 355,'Great Falls');
END
if not exists(select 1 from market_dma_map WHERE market_code = 356) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 356,'Billings');
END
if not exists(select 1 from market_dma_map WHERE market_code = 357) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 357,'Boise');
END
if not exists(select 1 from market_dma_map WHERE market_code = 358) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 358,'Idaho Falls');
END
if not exists(select 1 from market_dma_map WHERE market_code = 359) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 359,'Cheyenne');
END
if not exists(select 1 from market_dma_map WHERE market_code = 360) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 360,'Twin Falls');
END
if not exists(select 1 from market_dma_map WHERE market_code = 362) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 362,'Missoula');
END
if not exists(select 1 from market_dma_map WHERE market_code = 364) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 364,'Rapid City');
END
if not exists(select 1 from market_dma_map WHERE market_code = 365) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 365,'El Paso');
END
if not exists(select 1 from market_dma_map WHERE market_code = 366) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 366,'Helena');
END
if not exists(select 1 from market_dma_map WHERE market_code = 367) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 367,'Casper');
END
if not exists(select 1 from market_dma_map WHERE market_code = 370) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 370,'Salt Lake City-Ogden');
END
if not exists(select 1 from market_dma_map WHERE market_code = 371) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 371,'Yuma');
END
if not exists(select 1 from market_dma_map WHERE market_code = 373) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 373,'Grand Junction');
END
if not exists(select 1 from market_dma_map WHERE market_code = 389) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 389,'Tucson');
END
if not exists(select 1 from market_dma_map WHERE market_code = 390) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 390,'Albuquerque');
END
if not exists(select 1 from market_dma_map WHERE market_code = 398) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 398,'Glendive');
END
if not exists(select 1 from market_dma_map WHERE market_code = 400) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 400,'Bakersfield');
END
if not exists(select 1 from market_dma_map WHERE market_code = 401) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 401,'Eugene-Springfield');
END
if not exists(select 1 from market_dma_map WHERE market_code = 402) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 402,'Eureka');
END
if not exists(select 1 from market_dma_map WHERE market_code = 403) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 403,'Los Angeles');
END
if not exists(select 1 from market_dma_map WHERE market_code = 404) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 404,'Palm Springs, CA');
END
if not exists(select 1 from market_dma_map WHERE market_code = 407) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 407,'San Francisco');
END
if not exists(select 1 from market_dma_map WHERE market_code = 410) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 410,'Yakima');
END
if not exists(select 1 from market_dma_map WHERE market_code = 411) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 411,'Reno');
END
if not exists(select 1 from market_dma_map WHERE market_code = 413) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 413,'Medford-Ashland');
END
if not exists(select 1 from market_dma_map WHERE market_code = 419) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 419,'Seattle-Tacoma');
END
if not exists(select 1 from market_dma_map WHERE market_code = 420) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 420,'Portland, OR');
END
if not exists(select 1 from market_dma_map WHERE market_code = 421) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 421,'Bend/Redmond, OR');
END
if not exists(select 1 from market_dma_map WHERE market_code = 425) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 425,'San Diego');
END
if not exists(select 1 from market_dma_map WHERE market_code = 428) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 428,'Monterey-Salinas');
END
if not exists(select 1 from market_dma_map WHERE market_code = 439) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 439,'Las Vegas');
END
if not exists(select 1 from market_dma_map WHERE market_code = 455) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 455,'Santa Barbara');
END
if not exists(select 1 from market_dma_map WHERE market_code = 462) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 462,'Sacramento');
END
if not exists(select 1 from market_dma_map WHERE market_code = 466) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 466,'Fresno');
END
if not exists(select 1 from market_dma_map WHERE market_code = 468) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 468,'Chico');
END
if not exists(select 1 from market_dma_map WHERE market_code = 481) 
BEGIN
   INSERT INTO market_dma_map (market_code,dma_mapped_value) VALUES ( 481,'Spokane');
END

/*************************************** BCOP-1370 END   ***************************************************/

/*************************************** BCOP-1587 START ***************************************************/

IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'station_program_flight_proposal' and COLUMN_NAME = 'impressions' and DATA_TYPE = 'float')
BEGIN
	exec sp_executesql N'ALTER TABLE station_program_flight_proposal DROP COLUMN impressions'
END

/*************************************** BCOP-1587 END   ***************************************************/


/*************************************** BCOP-1677 START   ***************************************************/


IF NOT EXISTS(SELECT 1 FROM sys.columns 
              WHERE Name = 'open_market_impressions_total'
              AND Object_ID = Object_ID('proposal_version_details'))
BEGIN
	ALTER TABLE proposal_version_details
	ADD open_market_impressions_total INT NOT NULL CONSTRAINT DF_proposal_version_details_open_market_impressions_total DEFAULT (0)
END

IF NOT EXISTS(SELECT 1 FROM sys.columns 
              WHERE Name = 'open_market_cost_total'
              AND Object_ID = Object_ID('proposal_version_details'))
BEGIN
	ALTER TABLE proposal_version_details
	ADD open_market_cost_total MONEY NOT NULL CONSTRAINT DF_proposal_version_details_open_market_cost_total DEFAULT (0)
END

IF NOT EXISTS(SELECT 1 FROM sys.columns 
              WHERE Name = 'proprietary_impressions_total'
              AND Object_ID = Object_ID('proposal_version_details'))
BEGIN
	ALTER TABLE proposal_version_details
	ADD proprietary_impressions_total INT NOT NULL CONSTRAINT DF_proposal_version_details_proprietary_impressions_total DEFAULT (0)
END

IF NOT EXISTS(SELECT 1 FROM sys.columns 
              WHERE Name = 'proprietary_cost_total'
              AND Object_ID = Object_ID('proposal_version_details'))
BEGIN
	ALTER TABLE proposal_version_details
	ADD proprietary_cost_total MONEY NOT NULL CONSTRAINT DF_proposal_version_details_proprietary_cost_total DEFAULT (0)
END


/*************************************** BCOP-1677 END   ***************************************************/

/*************************************** BCOP-1666/1701 START ***************************************************/

IF not EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'proposal_versions' and COLUMN_NAME = 'target_cpm')
BEGIN
	ALTER TABLE proposal_versions ADD target_cpm money null
END

IF not EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'proposal_versions' and COLUMN_NAME = 'margin')
BEGIN
    ALTER TABLE proposal_versions ADD margin float	null
END

go

IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'proposal_versions' and COLUMN_NAME = 'target_cpm')
BEGIN
	update proposal_versions
	   set target_cpm = 0
	 where target_cpm is null;  
	ALTER TABLE proposal_versions ALTER COLUMN target_cpm money not null
END

IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'proposal_versions' and COLUMN_NAME = 'margin')
BEGIN
	update proposal_versions
	   set margin = 20
	 where margin is null;  
	ALTER TABLE proposal_versions ALTER COLUMN margin float not null
END

/*************************************** BCOP-1666/1701 END   ***************************************************/

/*************************************** BCOP-1724 START  ***************************************************/

/* Since the values are set to zero in EF by default, 
   the default constraints are not necessary. */
IF OBJECT_ID('DF_proposal_version_details_open_market_impressions_total') IS NOT NULL 
BEGIN
	ALTER TABLE proposal_version_details
	DROP CONSTRAINT DF_proposal_version_details_open_market_impressions_total
END

IF OBJECT_ID('DF_proposal_version_details_proprietary_impressions_total') IS NOT NULL 
BEGIN
	ALTER TABLE proposal_version_details
	DROP CONSTRAINT DF_proposal_version_details_proprietary_impressions_total
END

IF OBJECT_ID('DF_proposal_version_details_open_market_cost_total') IS NOT NULL 
BEGIN
	ALTER TABLE proposal_version_details
	DROP CONSTRAINT DF_proposal_version_details_open_market_cost_total
END

IF OBJECT_ID('DF_proposal_version_details_proprietary_cost_total') IS NOT NULL 
BEGIN
	ALTER TABLE proposal_version_details
	DROP CONSTRAINT DF_proposal_version_details_proprietary_cost_total
END

IF (SELECT DATA_TYPE 
	FROM INFORMATION_SCHEMA.COLUMNS
	WHERE 
		TABLE_NAME = 'proposal_version_details' AND 
		COLUMN_NAME = 'open_market_impressions_total') = 'int'
BEGIN
	ALTER TABLE proposal_version_details
	ALTER COLUMN open_market_impressions_total BIGINT NOT NULL;
END

IF (SELECT DATA_TYPE 
	FROM INFORMATION_SCHEMA.COLUMNS
	WHERE 
		TABLE_NAME = 'proposal_version_details' AND 
		COLUMN_NAME = 'proprietary_impressions_total') = 'int'
BEGIN
	ALTER TABLE proposal_version_details
	ALTER COLUMN proprietary_impressions_total BIGINT NOT NULL;
END

IF NOT EXISTS(SELECT 1 FROM sys.columns 
              WHERE Name = 'open_market_impressions_total'
              AND Object_ID = Object_ID('proposal_version_detail_quarter_weeks'))
BEGIN
	ALTER TABLE proposal_version_detail_quarter_weeks 
	ADD open_market_impressions_total BIGINT NOT NULL CONSTRAINT DF_proposal_version_detail_quarter_weeks_open_market_impressions_total DEFAULT (0)
END

IF NOT EXISTS(SELECT 1 FROM sys.columns 
              WHERE Name = 'open_market_cost_total'
              AND Object_ID = Object_ID('proposal_version_detail_quarter_weeks'))
BEGIN
	ALTER TABLE proposal_version_detail_quarter_weeks
	ADD open_market_cost_total MONEY NOT NULL CONSTRAINT DF_proposal_version_detail_quarter_weeks_open_market_cost_total DEFAULT (0)
END

IF NOT EXISTS(SELECT 1 FROM sys.columns 
              WHERE Name = 'proprietary_impressions_total'
              AND Object_ID = Object_ID('proposal_version_detail_quarter_weeks'))
BEGIN
	ALTER TABLE proposal_version_detail_quarter_weeks 
	ADD proprietary_impressions_total BIGINT NOT NULL CONSTRAINT DF_proposal_version_detail_quarter_weeks_proprietary_impressions_total DEFAULT (0)
END

IF NOT EXISTS(SELECT 1 FROM sys.columns 
              WHERE Name = 'proprietary_cost_total'
              AND Object_ID = Object_ID('proposal_version_detail_quarter_weeks'))
BEGIN
	ALTER TABLE proposal_version_detail_quarter_weeks 
	ADD  proprietary_cost_total MONEY NOT NULL CONSTRAINT DF_proposal_version_detail_quarter_weeks_proprietary_cost_total DEFAULT (0)
END

/* Since the values are set to zero in EF by default, 
   the default constraints are not necessary. */
IF OBJECT_ID('DF_proposal_version_details_open_market_impressions_total') IS NOT NULL 
BEGIN
	ALTER TABLE proposal_version_detail_quarter_weeks
	DROP CONSTRAINT DF_proposal_version_detail_quarter_weeks_open_market_impressions_total
END

IF OBJECT_ID('DF_proposal_version_details_open_market_impressions_total') IS NOT NULL 
BEGIN
	ALTER TABLE proposal_version_detail_quarter_weeks
	DROP CONSTRAINT DF_proposal_version_detail_quarter_weeks_open_market_cost_total
END

IF OBJECT_ID('DF_proposal_version_details_open_market_impressions_total') IS NOT NULL 
BEGIN
	ALTER TABLE proposal_version_detail_quarter_weeks
	DROP CONSTRAINT DF_proposal_version_detail_quarter_weeks_proprietary_impressions_total
END

IF OBJECT_ID('DF_proposal_version_details_open_market_impressions_total') IS NOT NULL 
BEGIN
	ALTER TABLE proposal_version_detail_quarter_weeks
	DROP CONSTRAINT DF_proposal_version_detail_quarter_weeks_proprietary_cost_total
END



/*************************************** BCOP-1724 END  ***************************************************/

/*************************************** BCOP-1663/1689 START ***************************************************/

IF not EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'station_program_flight_proposal' and COLUMN_NAME = 'isci')
BEGIN   
	ALTER TABLE station_program_flight_proposal ADD isci varchar(63) null
END

IF not EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'inventory_detail_slot_proposal' and COLUMN_NAME = 'isci')
BEGIN
    ALTER TABLE inventory_detail_slot_proposal ADD isci varchar(63) null
END

/*************************************** BCOP-1663/1689 END   ***************************************************/


/*************************************** END UPDATE SCRIPT *******************************************************/

------------------------------------------------------------------------------------------------------------------
------------------------------------------------------------------------------------------------------------------

-- Update the Schema Version of the database to the current release version
UPDATE system_component_parameters 
SET parameter_value = '5.8.9' -- Current release version
WHERE parameter_key = 'SchemaVersion'
GO

print 'Updated schema Version'

IF(XACT_STATE() = 1)
BEGIN
	
	IF EXISTS (SELECT TOP 1 * 
		FROM #previous_version 
		WHERE [version] = '5.8.8' -- Previous release version
		OR [version] = '5.8.9') -- Current release version
	BEGIN
		PRINT 'Database Successfully Updated'
		COMMIT TRANSACTION
		DROP TABLE #previous_version
	END
	ELSE
	BEGIN
		PRINT 'Incorrect Previous Database Version'
		ROLLBACK TRANSACTION
	END

END
GO

IF(XACT_STATE() = -1)
BEGIN
	ROLLBACK TRANSACTION
	PRINT 'Database Update Failed. Transaction rolled back.'
END
GO





































