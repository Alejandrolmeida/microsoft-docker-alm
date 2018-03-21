#!/bin/bash

CLIENT_ID=$(cat ~/.azure/mycredentials.json | jq -r ".clientId")
SECRET=$(cat ~/.azure/mycredentials.json | jq -r ".clientSecret")
TENANT_ID=$(cat ~/.azure/mycredentials.json | jq -r ".tenantId")
NAME=$(cat ~/.azure/serviceprincipal.json | jq -r ".servicePrincipalNames[0]")

az login --service-principal --username $NAME --password $SECRET --tenant $TENANT_ID

echo "export AZURE_TENANT_ID=$TENANT_ID" > ~/.azure/setvariables.sh
echo "export AZURE_CLIENT_ID=$CLIENT_ID" >> ~/.azure/setvariables.sh
echo "export AZURE_CLIENT_SECRET=$SECRET" >> ~/.azure/setvariables.sh
echo ""
echo "Ahora puede incluir las variables de entorno en su ~/.bashrc agregando la siguiente linea:"
echo ". ~/.azure/setvariables.sh"
echo ""