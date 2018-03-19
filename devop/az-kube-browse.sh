#!/bin/bash

RESOURCE_GROUP=$(cat ~/.azure/aksConfig.json | jq -r ".resourceGroup")
NAME=$(cat ~/.azure/aksConfig.json | jq -r ".name")

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

az aks browse \
        --resource-group $RESOURCE_GROUP \
        --name $NAME &


#az aks show -g microsoft-docker-alm --name kubecluster -o json | jq -r ".servicePrincipalProfile.clientId"