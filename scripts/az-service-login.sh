#!/bin/bash

CLIENT_ID=$(cat ~/.azure_alm/mycredentials.json | jq -r ".clientId")
SECRET=$(cat ~/.azure_alm/mycredentials.json | jq -r ".clientSecret")
TENANT_ID=$(cat ~/.azure_alm/mycredentials.json | jq -r ".tenantId")
NAME=$(cat ~/.azure_alm/serviceprincipal.json | jq -r ".servicePrincipalNames[0]")

az login --service-principal --username $NAME --password $SECRET --tenant $TENANT_ID

echo "export AZURE_TENANT_ID=$TENANT_ID" > ~/.azure_alm/setvariables.sh
echo "export AZURE_CLIENT_ID=$CLIENT_ID" >> ~/.azure_alm/setvariables.sh
echo "export AZURE_CLIENT_SECRET=$SECRET" >> ~/.azure_alm/setvariables.sh
echo ""
echo "Ahora puede incluir las variables de entorno en su ~/.bashrc agregando la siguiente linea:"
echo ". ~/.azure_alm/setvariables.sh"
echo ""