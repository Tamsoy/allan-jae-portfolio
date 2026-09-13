$ErrorActionPreference = 'Stop'
$taskRoot = $PSScriptRoot
$framework = Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319'
$compiler = Join-Path $framework 'csc.exe'
$buildDir = Join-Path $taskRoot 'build'
New-Item -ItemType Directory -Force $buildDir | Out-Null
$sources = @('App.cs','Model.cs','RecordedBank.cs','Audio.cs','Input.cs','SelfTest.cs','Version.cs') | ForEach-Object { Join-Path $taskRoot "src/$_" }
$refs = @('System.dll','System.Core.dll','System.Web.Extensions.dll','System.Windows.Forms.dll','System.Drawing.dll','System.Xaml.dll') | ForEach-Object { '/r:' + (Join-Path $framework $_) }
$refs += @('PresentationCore.dll','PresentationFramework.dll','WindowsBase.dll') | ForEach-Object { '/r:' + (Join-Path $framework "WPF/$_") }
& $compiler /nologo /target:winexe /platform:x64 /optimize+ /utf8output "/out:$buildDir/KeyTone.exe" "/win32manifest:$taskRoot/src/app.manifest" "/win32icon:$taskRoot/src/KeyTone.ico" "/resource:$taskRoot/src/Main.xaml,Main.xaml" "/resource:$taskRoot/src/KeyTone.ico,KeyTone.ico" "/resource:$taskRoot/assets/switches/blue.bin,blue.bin" "/resource:$taskRoot/assets/switches/red.bin,red.bin" "/resource:$taskRoot/assets/switches/brown.bin,brown.bin" @refs @sources
if ($LASTEXITCODE -ne 0) { throw 'App compilation failed' }
Copy-Item -LiteralPath (Join-Path $taskRoot 'src/KeyTone.exe.config') -Destination $buildDir
Copy-Item -LiteralPath (Join-Path $taskRoot 'README.txt'),(Join-Path $taskRoot 'LICENSE.txt'),(Join-Path $taskRoot 'THIRD-PARTY-NOTICES.txt') -Destination $buildDir
$resources = @()
foreach ($name in @('KeyTone.exe','KeyTone.exe.config','README.txt','LICENSE.txt','THIRD-PARTY-NOTICES.txt')) {
  $inputStream = [IO.File]::OpenRead((Join-Path $buildDir $name))
  $outputStream = [IO.File]::Create((Join-Path $buildDir "$name.gz"))
  $gzip = New-Object IO.Compression.GZipStream($outputStream, [IO.Compression.CompressionMode]::Compress)
  try { $inputStream.CopyTo($gzip) } finally { $gzip.Dispose(); $outputStream.Dispose(); $inputStream.Dispose() }
  $resources += "/resource:$buildDir/$name.gz,$name.gz"
}
& $compiler /nologo /target:winexe /platform:x64 /optimize+ "/out:$buildDir/KeyTone-Setup.exe" "/win32manifest:$taskRoot/src/app.manifest" "/win32icon:$taskRoot/src/KeyTone.ico" "/resource:$taskRoot/src/KeyTone.ico,KeyTone.ico" /r:System.dll /r:System.Core.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll /r:Microsoft.CSharp.dll @resources (Join-Path $taskRoot 'src/Setup.cs') (Join-Path $taskRoot 'src/Version.cs')
if ($LASTEXITCODE -ne 0) { throw 'Installer compilation failed' }
Write-Output "Built app and installer in $buildDir"
