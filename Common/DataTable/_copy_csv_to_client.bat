::enter to current folder
%~d0
cd %~dp0

:: remove the attribute 'read only' of csv files
attrib ..\..\Client\Snaker\Assets\StreamingAssets\DataTable\*.csv -r /s

:: delete old .txt files
del ..\..\Client\Snaker\Assets\StreamingAssets\DataTable\*.txt

:: copy new files
copy *.csv ..\..\Client\Snaker\Assets\StreamingAssets\DataTable

:: rename .csv to .txt
ren ..\..\Client\Snaker\Assets\StreamingAssets\DataTable\*.csv *.txt

pause