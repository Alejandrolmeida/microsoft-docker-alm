#!/bin/bash
# http://redsymbol.net/articles/unofficial-bash-strict-mode/
set -euo pipefail

c=$(docker container ls -a | grep "microsoft/mssql-server-linux" | awk '{print $1}')

if [[ -z $c ]]; then
  docker exec -it $c /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'Password1234'
fi