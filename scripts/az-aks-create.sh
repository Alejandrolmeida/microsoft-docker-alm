#!/bin/bash

usage()
{
    cat <<END
  
  az-aks-create.sh: Este script crea un cluster de Kubernetes en Azure Container Service (AKS)
  
  Ejemplo:
    # az-aks-create.sh -g microsoft-docker-alm -n kubecluster -c 1

  Parameters:
    -l <LOCATION>
      Especifica el nombre para la localizacion  
    -g <RESOURCE_GROUP>
      Especifica el nombre para el Grupo de Recursos   
    -n <NAME>
      Especifica el nombre para el cluster de Kubernetes   
    -c <NODE_COUNT>
      Especifica el numero de nodos que se ejecutaran en el cluster    
    -h | --help
      Muestra esta ayuda

  Nota:    
    Para la correcta ejecucion de este script es necesario tener las herramientas cliente CLI de Docker instaladas y el demonio de Docker corriendo.
    Para mas informacion https://docs.docker.com

END
}

while getopts :l:g:n:c:h: option; do
    case $option in
      g) RESOURCE_GROUP=${OPTARG};;      
      n) NAME=${OPTARG};;
      c) NODE_COUNT=${OPTARG};;
      h) usage; exit 1;;
      \?) echo "Invalid option: -$OPTARG" >&2; usage; exit 1;;
      :) echo "Option -$OPTARG requires an argument." >&2; usage; exit 1;;
    esac
done

if [[ ! $RESOURCE_GROUP ]]; then  
  echo "Debe especificar un Grupo de Recursos."
  usage
  exit 1
fi 
if [[ ! $NAME ]]; then  
  echo "Debe especificar un nombre par el ACR."
  usage
  exit 1
fi 
if [[ ! $NODE_COUNT ]]; then  
  echo "Debe especificar un numero de nodos para ejecutar en el cluster."
  usage
  exit 1
fi 

APP_ID=$(cat ~/.azure_alm/mycredentials.json  | jq -r ".clientId")
SECRET=$(cat ~/.azure_alm/mycredentials.json  | jq -r ".clientSecret")

## CLUSTER KUBERNETES
    # Creamos el cluster    
    az aks create \
        --resource-group $RESOURCE_GROUP \
        --name $NAME \
        --node-count $NODE_COUNT \
        --generate-ssh-keys \
        --service-principal $APP_ID \
        --client-secret $SECRET \
        --kubernetes-version "1.9.2" 

    # Guardamos la configuracion del servicio
    echo $(az aks show --resource-group $RESOURCE_GROUP --name $NAME) > ~/.azure_alm/aksConfig.json

    # Conectamos con el cluster
    az aks get-credentials \
        --resource-group $RESOURCE_GROUP \
        --name $NAME

    # # Creamos el conector para ACI
    # # Antes de activar esto leer:
    # # https://blog.jcorioland.io/archives/2017/11/27/how-to-use-azure-container-instance-connector-kubernetes-with-container-service-aks.html
    # # https://www.danielstechblog.info/deploying-kubernetes-aci-connector-aks-managed-kubernetes-azure/
    # # https://github.com/virtual-kubelet/virtual-kubelet/tree/master/providers/azure
    # az aks install-connector \
    #     --resource-group $RESOURCE_GROUP \
    #     --name $NAME \
    #     --connector-name $NAME"-aci" \
    #     --aci-resource-group $RESOURCE_GROUP \
    #     --service-principal "http://"$APP_ID \
    #     --client-secret $SECRET