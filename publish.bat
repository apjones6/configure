dotnet build -c Release
if not exist "build\" mkdir build
lib\ILMerge /ndebug /out:build\configure.exe Configure\bin\Release\net46\configure.exe Configure\bin\Release\net46\YamlDotNet.dll Configure\bin\Release\net46\System.Xml.XPath.dll Configure\bin\Release\net46\System.Xml.XmlDocument.dll