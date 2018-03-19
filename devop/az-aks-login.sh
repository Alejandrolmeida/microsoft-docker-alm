#!/bin/bash

RESOURCE_GROUP=$(cat ~/.azure/aksConfig.json  | jq -r ".resourceGroup")
NAME=$(cat ~/.azure/aksConfig.json  | jq -r ".name")


if [[ ! $RESOURCE_GROUP ]]; then  
  echo "No se encuetra definicion de RESOURCE_GROUP. Revise el documento ~/.azure/azureregistry.json"
  usage
  exit 1
fi 

if [[ ! $NAME ]]; then  
  echo "No se encuetra definicion de NAME. Revise el documento ~/.azure/azureregistry.json"
  usage
  exit 1
fi 

# Conectamos al cluster de Kubernetes
az aks get-credentials \
    --resource-group $RESOURCE_GROUP \
    --name $NAME