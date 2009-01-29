#!/bin/sh

xsp2 --root /var/www/mono/ --nonstop &

echo waiting 3 seconds
sleep 3

echo compiling wsdl
wsdl2 http://localhost:8080/chronojumpServer.asmx
#wsdl2 http://0.0.0.0:8080/chronojumpServer.asmx
