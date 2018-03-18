#!/bin/bash

jsonValue() 
{
    KEY=$1
    num=$2
    awk -F"[,:}]" '{for(i=1;i<=NF;i++){if($i~/'$KEY'\042/){print $(i+1)}}}' | tr -d ' ' | tr -d '"'
}

SCOPE=$(cat ~/.azure/acrConfig.json | jsonValue id)
CLIENT_ID=$(cat ~/.azure/mycredentials.json | jsonValue clientId)

az role assignment create \
        --assignee $CLIENT_ID \
        --role=Contributor \
        --scope=$SCOPE