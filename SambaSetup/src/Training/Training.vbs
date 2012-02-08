' SambaPOS Training V3.0
'
' Developed by John Sheather (aka JohnSCS) for SambaPOS
' 2-Feb-2012
'
' Forum Support - http://forum2.sambapos.com/index.php/topic,184.0.html
' Web Site - http://www.sambapos.com/en
'
'
strDelTrSQLCE = False	' Change this to True to automatically refresh the Training Database each time Training is run.

Const ForReading = 1    
Const ForWriting = 2

dim sampletext, objRegExp, SearchPattern, ReplacePattern, matches 
dim outputArray, inputText, message, strDS, strIS, intReturn
Dim objFSO, objFile, match, strFileName, strE2CE1, strE2CE2, strE2CE3, strE2CE4
dim fso, oShell, strUserProfile, strSSLoc
Dim sCurPath

sCurPath = CreateObject("Scripting.FileSystemObject").GetAbsolutePathName(".")
strTrSQLCE = sCurPath & "\SambaData2.sdf"
strSamba = strInDir & "Samba.Presentation.exe"

sParentPath = ""
sPath = split(sCurPath,"\")
for i = 0 to UBound(sPath)-1
	sParentPath = sParentPath & sPath(i) & "\"
next

Set fso = CreateObject("Scripting.FileSystemObject")
Set oShell = CreateObject("Wscript.Shell")
strUserProfile = oShell.ExpandEnvironmentStrings("%USERPROFILE%") 

' Check that Compact SQL is installed

If (fso.FolderExists("C:\Program Files\Microsoft SQL Server Compact Edition")=False) Then
	message = "SambaPOS Training Requires Microsoft SQL Server Compact V4.0 to be installed."
	response = msgbox (message,16,"SambaPOS Training")
	Wscript.Quit
End if

' Find SambaSettings.txt file

If (fso.FileExists(strUserProfile&"\AppData\Roaming\Ozgu Tech\SambaPOS2\SambaSettings.txt")) Then
	strSSLoc = strUserProfile&"\AppData\Roaming\Ozgu Tech\SambaPOS2\SambaSettings.txt"
Else if (fso.FileExists(strUserProfile&"\Application Data\Ozgu Tech\SambaPOS2\SambaSettings.txt")) Then
	strSSLoc = strUserProfile&"\Application Data\Ozgu Tech\SambaPOS2\SambaSettings.txt"
Else if (fso.FileExists("C:\ProgramData\Ozgu Tech\SambaPOS2\SambaSettings.txt")) Then
	strSSLoc = "C:\ProgramData\Ozgu Tech\SambaPOS2\SambaSettings.txt"
Else if (fso.FileExists("C:\Documents and Settings\All Users\Application Data\Ozgu Tech\SambaPOS2\SambaSettings.txt")) Then
	strSSLoc = "C:\Documents and Settings\All Users\Application Data\Ozgu Tech\SambaPOS2\SambaSettings.txt"
End If
End If
End If
End If

' Get data connection string

Set objFSO = CreateObject("Scripting.FileSystemObject") 
Set objFile = objFSO.OpenTextFile(strSSLoc, ForReading) 
Do Until objFile.AtEndOfStream 
	sampletext = objFile.ReadAll 
	SearchPattern = "<ConnectionString>" 
	SearchPattern = SearchPattern & "(.*?)([\s\S]*?)" 
	SearchPattern = SearchPattern & "</ConnectionString>" 
	Set objRegExp = New RegExp 
	objRegExp.Pattern = SearchPattern ' apply the search pattern 
	objRegExp.Global = True ' match all instances if the serach pattern 
	objRegExp.IgnoreCase = True ' ignore case 
	Set matches = objRegExp.execute(sampletext) 
	If matches.Count > 0 Then
		For Each match in matches
			inputText=Split(Split(match.Value, ">")(1), "<")(0) 
		Next 
	Else ' there were no matches found 
	wscript.echo objRegExp.Pattern & "was not found in the string" 
	End If
Loop 
Set objRegExp = Nothing 

' Check if user is running TXT file database

if Instr(1,inputText,".txt",vbTextCompare) > 0 then
	message = "-- SambaPOS Training Does Not Work With TXT Files --" & vbCRLF & vbCRLF & "Please install either" & vbCRLF & vbCRLF & "Microsoft SQL Server Compact 4.0 (Standalone POS)" & vbCRLF & "or" & vbCRLF & "Microsoft SQL Server 2008 Express (Networked POS)"
	response = msgbox (message,16,"SambaPOS Training")
	Wscript.Quit
End if

' check for valid data connection string

if Instr(1,inputText,"data source",vbTextCompare) > 0 then
	strDS = Replace(inputText, "data source=", "")
	if Instr(1,strDS,"password",vbTextCompare) > 0 then
		strIS = "False"
	Else
		strIS = "True"
	End if
Else
	message = "SambaPOS Training can not find a" & vbCRLF & "valid Data Connection String" & vbCRLF & vbCRLF & "Please run SambaPOS, go to Manage -> Local Settings and click save."
	response = msgbox (message,16,"SambaPOS Training")
	Wscript.Quit

End if



' Migrate Database

if (fso.FileExists(strTrSQLCE)=False) Then
	if Instr(1,strDS,".sdf",vbTextCompare) > 0 then
		Set fso = CreateObject("Scripting.FileSystemObject")
		fso.CopyFile strDS, strTrSQLCE, True	
	Else
		Set fso = CreateObject("Scripting.FileSystemObject")

		Set WshShell = WScript.CreateObject("WScript.Shell")

		strE2CE1 = "Export2sqlce " & chr(34) & "Data Source=" & strDS & ";Initial Catalog=SambaData2;Integrated Security=" & strIS & ";" & chr(34) & " SambaData2.sqlce"
		strE2CE2 = "sqlcecmd40 -d " & chr(34) & "Data Source=SambaData2.sdf" & chr(34) & " -e create"
		strE2CE3 = "cmd /c sqlcecmd40 -d " & chr(34) & "Data Source=SambaData2.sdf" & chr(34) & " -i SambaData2.sqlce > log.txt"

		intReturn = WshShell.Run(strE2CE1, 2, TRUE)
		intReturn = WshShell.Run(strE2CE2, 2, TRUE)
		intReturn = WshShell.Run(strE2CE3, 2, TRUE)

		if (fso.FileExists("log.txt")) Then
			fso.DeleteFile "log.txt", True
		End if
		if (fso.FileExists("SambaData2.sqlce")) Then
			fso.DeleteFile "SambaData2.sqlce", True
		End if
	End if
End If


' Backup SambaSettings.txt incase of an issue

Set fso = CreateObject("Scripting.FileSystemObject")
fso.CopyFile strSSLoc, strSSLoc & ".bak", True	

' Change SambaSettings.txt to SQLCE DB

strFileName = strSSLoc
strOldText = strDS
strNewText = strTrSQLCE

Set objFSO = CreateObject("Scripting.FileSystemObject")
Set objFile = objFSO.OpenTextFile(strFileName, ForReading)
strText = objFile.ReadAll
objFile.Close

strNewText = Replace(strText, strOldText, strNewText)
Set objFile = objFSO.OpenTextFile(strFileName, ForWriting)
objFile.WriteLine strNewText
objFile.Close


' Run SambaPOS

Set WshShell = WScript.CreateObject("WScript.Shell")
intReturn = WshShell.Run(chr(34) & sParentPath & strSamba & chr(34), 3, TRUE)


' Change SambaSettings.txt to original setting

strFileName = strSSLoc
strNewText = strDS
strOldText = strTrSQLCE

Set objFSO = CreateObject("Scripting.FileSystemObject")
Set objFile = objFSO.OpenTextFile(strFileName, ForReading)
strText = objFile.ReadAll
objFile.Close

strNewText = Replace(strText, strOldText, strNewText)
Set objFile = objFSO.OpenTextFile(strFileName, ForWriting)
objFile.WriteLine strNewText
objFile.Close


' Delete Training SQLCE File?

if (strDelTrSQLCE) then
	Set fso = CreateObject("Scripting.FileSystemObject")
	fso.DeleteFile strTrSQLCE, True
End if