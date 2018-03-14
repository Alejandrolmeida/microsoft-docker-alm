docker container rm -f $(docker container ls -a | grep "microsoft/mssql-server-linux" | awk '{print $1}')

docker exec -it f2f75f544a1b /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'P@ssw0rd'