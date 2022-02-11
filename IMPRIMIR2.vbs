On Error Resume Next
If Not IsObject(application) Then
   Set SapGuiAuto  = GetObject("SAPGUI")
   Set application = SapGuiAuto.GetScriptingEngine
End If
If Not IsObject(connection) Then
   Set connection = application.Children(0)
End If
If Not IsObject(session) Then
   Set session    = connection.Children(0)
End If
If IsObject(WScript) Then
   WScript.ConnectObject session,     "on"
   WScript.ConnectObject application, "on"
End If
session.findById("wnd[0]").maximize
session.findById("wnd[0]/usr/shell").contextMenu
session.findById("wnd[0]/usr/shell").selectContextMenuItem "&XXL"
session.findById("wnd[1]/tbar[0]/btn[0]").press
session.findById("wnd[1]/usr/ctxtDY_PATH").text = "$DESTINO$"
session.findById("wnd[1]/usr/ctxtDY_FILENAME").text = "$NOME$"
session.findById("wnd[1]/usr/ctxtDY_FILENAME").caretPosition = $TAM$
session.findById("wnd[1]/tbar[0]/btn[11]").press

