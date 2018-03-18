#!/bin/bash

jsonValue() 
{
    KEY=$1
    num=$2
    awk -F"[,:}]" '{for(i=1;i<=NF;i++){if($i~/'$KEY'\042/){print $(i+1)}}}' | tr -d ' ' | tr -d '"'
}

CLIENT_ID=$(cat ~/.azure/mycredentials.json | jsonValue clientId)
SECRET=$(cat ~/.azure/mycredentials.json | jsonValue clientSecret)
TENANT_ID=$(cat ~/.azure/mycredentials.json | jsonValue tenantId)
NAME=$(cat ~/.azure/serviceprincipal.json | jsonValue appDisplayName)

az login --service-principal --username "http://"$NAME --password $SECRET --tenant $TENANT_ID

echo "export AZURE_TENANT_ID=$TENANT_ID" > ~/.azure/setvariables.sh
echo "export AZURE_CLIENT_ID=$CLIENT_ID" >> ~/.azure/setvariables.sh
echo "export AZURE_CLIENT_SECRET=$SECRET" >> ~/.azure/setvariables.sh
echo ""
echo "Ahora puede incluir las variables de entorno en su ~/.bashrc agregando la siguiente linea:"
echo ". ~/.azure/setvariables.sh"
echo ""