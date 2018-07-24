@setlocal enableextensions
@cd /d "%~dp0"
certutil -addstore "TrustedPublisher" kopiocert.cer
certutil -addstore "Root" kopiocert.cer
pause