#!/bin/bash

az-acr-login
docker tag selfiefrontend:latest alejandrolmeida.azurecr.io/selfiefrontend:v3
docker tag selfiebackend:latest alejandrolmeida.azurecr.io/selfiebackend:v3
docker tag storagevaletkey:latest alejandrolmeida.azurecr.io/storagevaletkey:v3

docker push alejandrolmeida.azurecr.io/selfiefrontend:v3
docker push alejandrolmeida.azurecr.io/selfiebackend:v3
docker push alejandrolmeida.azurecr.io/storagevaletkey:v3
