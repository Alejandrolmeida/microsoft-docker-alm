#!/bin/bash

c=$(docker container ls -a | grep "microsoft/mssql-server-linux" | awk '{print $1}')

for i in $c
do
  docker container rm -f $i
done

docker pull microsoft/mssql-server-linux:latest
docker run -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=P@ssw0rd' -p 1433:1433 -d microsoft/mssql-server-linux:2017-latest
