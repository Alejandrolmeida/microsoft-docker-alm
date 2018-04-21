az-acr-login
docker tag selfiefrontend:latest alejandrolmeida.azurecr.io/selfiefrontend:v2
docker tag selfiebackend:latest alejandrolmeida.azurecr.io/selfiebackend:v2
docker tag storagevaletkey:latest alejandrolmeida.azurecr.io/storagevaletkey:v2

docker push alejandrolmeida.azurecr.io/selfiefrontend:v2
docker push alejandrolmeida.azurecr.io/selfiebackend:v2
docker push alejandrolmeida.azurecr.io/storagevaletkey:v2
