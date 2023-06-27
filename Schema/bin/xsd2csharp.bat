@echo off

@rem Save the current directory
set OLDDIR=%CD%
echo Running batch file: %0
echo Running batch file at location: %~dp0

@rem Change to the directory relative to the location of this BAT file
cd %~dp0

@rem Set the default control arguments
cls
set SCHEMA_VERSION=0.9

@rem Store the output directory
if NOT "%1" == "" set SCHEMA_VERSION=%1
echo Processing schema version: %SCHEMA_VERSION%

set SCHEMA_VERSION_NODOT=%SCHEMA_VERSION:.=_%
set OUTPUT_SCHEMA_CLASS_DIR=..\..\Unity\Assets\MRET\Core\Schema\v%SCHEMA_VERSION%

@rem Batch file variables
@rem :process_arguments
@rem     set arg=%1
@rem     if "%arg:,1%" == "/" (
@rem         if "/i" %arg% EQU "/v" (
@rem             set verbose=YES
@rem         ) else if "/i" %arg% EQU "/q" (
@rem             set quiet=YES
@rem         ) else (
@rem             goto usage
@rem         )
@rem         shift /1
@rem         goto process_arguments
@rem     )

@rem Make sure the output directory exists
if not exist "%OUTPUT_SCHEMA_CLASS_DIR%" (
	mkdir %OUTPUT_SCHEMA_CLASS_DIR%
	echo Creating %OUTPUT_SCHEMA_CLASS_DIR%
)
echo Output directory: %OUTPUT_SCHEMA_CLASS_DIR%

@rem Build the list of schema files for this schema version
@rem cd ..\XSD\SchemaFiles\v%SCHEMA_VERSION%
set XSD_FILES=
set CURRENT_PATH=%__CD__%
for /f "delims=*" %%a in ('dir /s/b ..\XSD\SchemaFiles\v%SCHEMA_VERSION%\*.xsd') do (
	set FILE=%%a
	setlocal enabledelayedexpansion
	@rem Convert the filename into a relative path by removing the current path
	set XSD_FILES=!XSD_FILES! !FILE:%CURRENT_PATH%=!
	endlocal
)

@rem Specify the schemas we are converting because we can't use the logic above
@rem due to the multiple imports of the same types
if %SCHEMA_VERSION% == 0.1 (
	set XSD_CUSTOM_FILES= ^
		..\XSD\SchemaFiles\v%SCHEMA_VERSION%\TimeSimulation.xsd ^
		..\XSD\SchemaFiles\v%SCHEMA_VERSION%\TextAnnotation.xsd ^
		..\XSD\SchemaFiles\v%SCHEMA_VERSION%\StaticPointCloud.xsd ^
		..\XSD\SchemaFiles\v%SCHEMA_VERSION%\Part.xsd ^
		..\XSD\SchemaFiles\v%SCHEMA_VERSION%\Mode.xsd ^
		..\XSD\SchemaFiles\v%SCHEMA_VERSION%\HUD.xsd ^
		..\XSD\SchemaFiles\v%SCHEMA_VERSION%\Configuration.xsd ^
		..\XSD\SchemaFiles\v%SCHEMA_VERSION%\AudioRecording.xsd ^
		..\XSD\SchemaFiles\v%SCHEMA_VERSION%\Animation.xsd
) else (
	set XSD_CUSTOM_FILES= ^
		..\XSD\SchemaFiles\v%SCHEMA_VERSION%\Types\Common\CommonTypes.xsd ^
		..\XSD\SchemaFiles\v%SCHEMA_VERSION%\TimeSimulation.xsd ^
		..\XSD\SchemaFiles\v%SCHEMA_VERSION%\TextAnnotation.xsd ^
		..\XSD\SchemaFiles\v%SCHEMA_VERSION%\Terrain.xsd ^
		..\XSD\SchemaFiles\v%SCHEMA_VERSION%\SceneObjectGenerator.xsd ^
		..\XSD\SchemaFiles\v%SCHEMA_VERSION%\PointCloud.xsd ^
		..\XSD\SchemaFiles\v%SCHEMA_VERSION%\Part.xsd ^
		..\XSD\SchemaFiles\v%SCHEMA_VERSION%\HUD.xsd ^
		..\XSD\SchemaFiles\v%SCHEMA_VERSION%\AudioAnnotation.xsd ^
		..\XSD\SchemaFiles\v%SCHEMA_VERSION%\ActionSequence.xsd
)
set XSD_FILES=%XSD_CUSTOM_FILES% ..\XSD\SchemaFiles\v%SCHEMA_VERSION%\Project.xsd

@rem Convert XSD files to C#
echo Starting XSD Conversion on schema files: %XSD_FILES%
xsd ^
	%XSD_FILES% ^
	/classes /language:CS /outputdir:%OUTPUT_SCHEMA_CLASS_DIR%

@rem Rename the output source file
cd %OUTPUT_SCHEMA_CLASS_DIR%
set SOURCE_CS=Project.cs
@rem set DEST_CS=SchemaTypes_v%SCHEMA_VERSION_NODOT%.cs
set DEST_CS=SchemaTypes.cs
if exist %SOURCE_CS% (
	if exist %DEST_CS% (
		del %DEST_CS%
	)
	echo Renaming %SOURCE_CS% to %DEST_CS%
	rename %SOURCE_CS% %DEST_CS%
	echo Source file created: %OUTPUT_SCHEMA_CLASS_DIR%\%DEST_CS%
) else (
	echo on Cannot locate converted class file: %SOURCE_CS%
)

@rem restore current directory
chdir /d %OLDDIR%
goto end

:usage
	echo Usage: %0 schemaVersion
	echo.
	echo Where:
	echo schemaVersion = schema version number. Default is '0.9'.
	echo.
	echo This batch file converts the schema files associated
	echo with the supplied version number to csharp classes and
	echo outputs them to the specified output folder.
	exit /b
	
:end