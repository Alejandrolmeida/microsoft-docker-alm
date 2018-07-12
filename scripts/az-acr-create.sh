#!/bin/bash

usage()
{
    cat <<END
  
  az-acr-create.sh: Este script crea un recurso de Azure Container Registry
  
  Ejemplo:
    # az-acr-create.sh -l eastus -g microsoft-docker-alm -n alejandrolmeida -s Basic    

  Parameters:
    -l <LOCATION>
      Especifica el nombre para la localizacion  
    -g <RESOURCE_GROUP>
      Especifica el nombre para el Grupo de Recursos   
    -n <CONTAINER_REGISTRY_NAME>
      Especifica el nombre para el registro de contenedores    
    -s <SKU>
      Especifica el nombre para el registro de contenedores    
    -h | --help
      Muestra esta ayuda

  Nota:    
    Para la correcta ejecucion de este script es necesario tener las herramientas cliente CLI de Docker instaladas y el demonio de Docker corriendo.
    Para mas informacion https://docs.docker.com

END
}

while getopts :l:g:n:s:h: option; do
    case $option in
      l) LOCATION=${OPTARG};;
      g) RESOURCE_GROUP=${OPTARG};;      
      n) CONTAINER_REGISTRY_NAME=${OPTARG};;
      s) SKU=${OPTARG};;
      h) usage; exit 1;;
      \?) echo "Invalid option: -$OPTARG" >&2; usage; exit 1;;
      :) echo "Option -$OPTARG requires an argument." >&2; usage; exit 1;;
    esac
done

if [[ ! $LOCATION ]]; then  
  echo "Debe especificar una localizacion."
  usage
  exit 1
fi 
if [[ ! $RESOURCE_GROUP ]]; then  
  echo "Debe especificar un Grupo de Recursos."
  usage
  exit 1
fi 
if [[ ! $CONTAINER_REGISTRY_NAME ]]; then  
  echo "Debe especificar un nombre par el ACR."
  usage
  exit 1
fi 
if [[ ! $SKU ]]; then  
  echo "Debe especificar un nivel de servicio (SKU)."
  usage
  exit 1
fi 

## CONTAINER REGISTRY
    # Creamos el Grupo de Recursos
    az group create -l $LOCATION -n $RESOURCE_GROUP

    # Creamos el Container Registry
    az acr create \
        --location $LOCATION \
        --resource-group $RESOURCE_GROUP\
        --name $CONTAINER_REGISTRY_NAME \
        --sku $SKU \
        --admin-enabled true

    # Conectamos con el servicio de Container Registry
    az acr login \
        --resource-group $RESOURCE_GROUP \
        --name $CONTAINER_REGISTRY_NAME

    # Guardamos la configuracion del servicio
    echo $(az acr show --name $CONTAINER_REGISTRY_NAME --resource-group $RESOURCE_GROUP) > ~/.azure_alm/acrConfig.json

    # Guardamos las credenciales del servicio
    echo $(az acr credential show -n $CONTAINER_REGISTRY_NAME --resource-group $RESOURCE_GROUP) > ~/.azure_alm/acrCredentials.json

    # Guardamos la configuracion del registro por defecto
    az configure --defaults acr=$CONTAINER_REGISTRY_NAME 