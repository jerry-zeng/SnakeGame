::enter to current folder
%~d0
::echo %~dp0
cd %~dp0

:: remove the attribute 'read only' of csv files
attrib *.csv -r /s

:: delete old .txt files
del ..\..\Client\Snake\Assets\Resources\DataTable\*.txt

:: copy new files
copy *.csv ..\..\Client\Snake\Assets\Resources\DataTable

:: rename .csv to .txt
ren ..\..\Client\Snake\Assets\Resources\DataTable\*.csv *.txt

pause