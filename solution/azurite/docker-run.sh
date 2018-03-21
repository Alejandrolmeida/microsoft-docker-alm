#!/bin/bash

c=$(docker container ls -a | grep "arafato/azurite" | awk '{print $1}')

for i in $c
do
  docker container rm -f $i
done

docker pull arafato/azurite:latest
docker run -d -t -p 10000:10000 -p 10001:10001 arafato/azurite
