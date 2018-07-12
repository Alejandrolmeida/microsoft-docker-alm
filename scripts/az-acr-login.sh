#!/bin/bash

RESOURCE_GROUP=$(cat ~/.azure_alm/acrConfig.json | jq -r ".resourceGroup")
NAME=$(cat ~/.azure_alm/acrCredentials.json | jq -r ".username")
PASSWORD=$(cat ~/.azure_alm/acrCredentials.json | jq -r ".passwords[0].value")

if [[ ! $RESOURCE_GROUP ]]; then  
  echo "No se encuetra definicion de RESOURCE_GROUP. Revise el documento ~/.azure_alm/acrConfig.json"
  usage
  exit 1
fi 

if [[ ! $NAME ]]; then  
  echo "No se encuetra definicion de NAME. Revise el documento ~/.azure_alm/acrCredentials.json"
  usage
  exit 1
fi 

if [[ ! $PASSWORD ]]; then  
  echo "No se encuetra definicion de PASSWORD. Revise el documento ~/.azure_alm/acrCredentials.json"
  usage
  exit 1
fi 

# Conectamos con el servicio de Container Registry
az acr login \
        --name $NAME \
        --resource-group $RESOURCE_GROUP \
        -n $NAME