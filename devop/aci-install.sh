    #Deployment of the ACI provider in your cluster
    export VK_RELEASE=virtual-kubelet-for-aks-0.1.3

    RELEASE_NAME=virtual-kubelet
    NODE_NAME=virtual-kubelet
    CHART_URL=https://github.com/virtual-kubelet/virtual-kubelet/raw/master/charts/$VK_RELEASE.tgz

    #curl https://raw.githubusercontent.com/virtual-kubelet/virtual-kubelet/master/scripts/createCertAndKey.sh | bash

    # Generate cert and key for chart
    mkdir -p ~/.cert
    openssl req -newkey rsa:4096 -new -nodes -x509 -days 3650 -keyout ~/.cert/key.pem -out ~/.cert/cert.pem -subj "/C=US/ST=CA/L=virtualkubelet/O=virtualkubelet/OU=virtualkubelet/CN=virtualkubelet"
    cert=$(base64 ~/.cert/cert.pem -w0)
    key=$(base64 ~/.cert/key.pem -w0)

    helm install "$CHART_URL" --name "$RELEASE_NAME" \
        --set env.azureClientId="$AZURE_CLIENT_ID",\
        env.azureClientKey="$AZURE_CLIENT_SECRET",\
        env.azureTenantId="$AZURE_TENANT_ID",\
        env.azureSubscriptionId="$AZURE_SUBSCRIPTION_ID",\
        env.aciResourceGroup="$RESOURCE_GROUP ",\
        env.nodeName="$NODE_NAME",\
        env.nodeOsType=Linux,\
        env.apiserverCert=$cert,\
        env.apiserverKey=$key