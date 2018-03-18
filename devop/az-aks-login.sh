#!/bin/bash

jsonValue() 
{
    KEY=$1
    num=$2
    awk -F"[,:}]" '{for(i=1;i<=NF;i++){if($i~/'$KEY'\042/){print $(i+1)}}}' | tr -d ' ' | tr -d '"'
}

RESOURCE_GROUP=$(cat ~/.azure/aksConfig.json | jsonValue resourceGroup)
NAME=$(cat ~/.azure/aksConfig.json | jsonValue id)

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
    --name ${NAME#*/*/*/*/*/*/*/*/*}