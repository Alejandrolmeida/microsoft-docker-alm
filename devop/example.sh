#!/bin/bash
## PREPARE AZURE
    # Logueamos con el login interactivo para crear nuestro servicio principal
    az login

    # Seleccionamos la suscripcion "Azure Sponsorship - David"
    az account set --subscription 125b7cd1-9b7a-41ea-bcb7-c312bf696932

    # Creamos un servicio principal con nuestro certificado
    az ad sp create-for-rbac \
        --name kubernetesapp \
        --cert @"/root/.azure/cert.pem" \
        --out tsv | cut -f1
   
    # Creamos el grupo de recursos
    az group create \
            --name Kubernetes \
            --location westeurope \
            --out tsv | cut -f1

    # Mostramos informacion los roles
    az role definition list --out table | grep Owner
    az role definition list --name Owner    

    # Asignamos permisos a este grupo al servicio principal
    az role assignment create \
        --assignee http://kubernetesapp \
        --role Owner \
        --scope "/subscriptions/125b7cd1-9b7a-41ea-bcb7-c312bf696932" 

    # TODO: falta crear el secret

    # Comprobamos la autenticacion con el servicio principal
    az login --service-principal \
        -u http://kubernetesapp \
        -p @"~/.azure/secret" \
        --tenant intelequia.com

## CONTAINER REGISTRY
    # Creamos el Container Registry
    az acr create \
        --location westeurope \
        --resource-group Kubernetes \
        --name alejandrolmeida \
        --sku Basic \
        --admin-enabled true

    # Conectamos con el servicio de Container Registry
    az acr login \
        --resource-group Kubernetes \
        --name alejandrolmeida
    
    # Obtenemos el nombre de loginServer 
    az acr list --resource-group Kubernetes --query "[].{acrLoginServer:loginServer}" --output table

    # Etiquetamos la version de la imagen
    docker tag webkul/qloapps_docker alejandrolmeida.azurecr.io/qloapps:v1

    # Enviamos la imagen al servicio de Container Registry
    docker push alejandrolmeida.azurecr.io/qloapps:v1

    # Listamos las imagenes en el repositorio
    az acr repository list --name alejandrolmeida --output table

## CLUSTER KUBERNETES
    # Instalamos el CLI de Kubernetes
    az aks install-cli

    # Creamos el cluster        
    az aks create \
        -l westeurope \
        --resource-group Kubernetes \
        --name KubeCluster \
        --admin-username alejandrolmeida \
        --dns-name-prefix kubecluster \
        --node-count 1 \
        --service-principal http://kubernetesapp \
        --client-secret @"~/.azure/secret" \
        --generate-ssh-keys \
        --kubernetes-version 1.8.7 \
        --node-vm-size Standard_DS1_v2 \
        --no-wait

    # Conectamos al cluster de Kubernetes
    az aks get-credentials \
        --resource-group Kubernetes \
        --name KubeCluster

    # kubectl create secret docker-registry hotelequiacr \
    #     --docker-server=hotelequiacr.azurecr.io \
    #     --docker-username=HotelequiaCR \
    #     --docker-password=SJS1ey+Ojx4NScdxQJZWkSGHeV6CThVm \
    #     --docker-email=alejandro.almeida@intelequia.com

    # Probamos la conexion mostrando los nodos del cluster
    kubectl get nodes
 
    # Creamos el archivo de manifiesto de publicación
    nano qloapps_publish.yaml

    # Publicamos la implementacion en el cluster
    kubectl create -f ~/github/kubernetes/qloapps_publish.yaml

    # Comprobamos la ejecucion
    kubectl get service qloapps --watch

    # Mostramos el Dashboard
    az aks browse --resource-group Kubernetes --name KubeCluster

    # Eliminamos la implementacion del cluster
    kubectl delete -f ~/github/kubernetes/qloapps_publish.yaml

    # Mostramos datos del cluster
    az aks show --resource-group Kubernetes --name KubeCluster --output table

    # Elimnamos los recursos
    #az aks delete --resource-group Kubernetes --name KubeCluster --no-wait --y    