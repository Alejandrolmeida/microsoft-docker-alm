#!/bin/bash

usage()
{
    cat <<END
  
  az-service-create.sh: Este script crea un servicio principal con el rol de acceso indicado
  
  Ejemplo:
    # az-service-create.sh -p kubealm-sp -s "MCT Alejandro Almeida" -r owner

  Parameters:
    -p <SERVICE_PRINCIPAL_NAME>
      Especifica el nombre del servicio principal  
    -s <SUBSCRIPTION> 
      Especifica el nombre de la suscripcion
    -r <ROLE> 
      Establece los permisos para el servicio principal. Debe tenerse en cuenta que 
      este rol permitira realizar diferentes acciones sobre Azure Container Registry:
        reader:      pull only
        contributor: push and pull
        owner:       push, pull, and assign roles
    -h | --help
      Muestra esta ayuda

END
}

while getopts :p:s:r:h: option; do
    case $option in
      p) SERVICE_PRINCIPAL_NAME=${OPTARG};;
      s) $(az account set --subscription "${OPTARG}"); SUBSCRIPTION_ID=$(az account show --query id);;
      r) ROLE=${OPTARG};;
      h) usage; exit 1;;
      \?) echo "Invalid option: -$OPTARG" >&2; usage; exit 1;;
      :) echo "Option -$OPTARG requires an argument." >&2; usage; exit 1;;
    esac
done

if [[ ! $SERVICE_PRINCIPAL_NAME ]]; then  
  echo "Debe especificar un nombre para el servicio principal"
  usage
  exit 1
fi 

if [[ ! $SUBSCRIPTION_ID ]]; then  
  echo "Debe especificar una subscripcion"
  usage
  exit 1
fi 

JSON=$(az ad sp create-for-rbac --name $SERVICE_PRINCIPAL_NAME --sdk-auth --scopes /subscriptions/${SUBSCRIPTION_ID//\"} --role $ROLE)

if [[ ! $JSON ]]; then  
  echo "No se ha podido crear el fichero de configuracion del servicio principal en  ~/.azure_alm/mycredentials.json"
  usage
  exit 1
fi 

echo "Guardando credenciales en ~/.azure_alm/mycredentials.json"
echo $JSON > ~/.azure_alm/mycredentials.json

CLIENT_ID=$(echo $JSON | jq -r ".clientId")
echo "Guardando configuracion en ~/.azure_alm/serviceprincipal.json"
az ad sp show --id $CLIENT_ID > ~/.azure_alm/serviceprincipal.json 