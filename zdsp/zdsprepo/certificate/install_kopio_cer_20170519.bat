@setlocal enableextensions
@cd /d "%~dp0"
certutil -addstore "TrustedPublisher" kopio_cer_20170519.cer
certutil -addstore "Root" kopio_cer_20170519.cer
pause