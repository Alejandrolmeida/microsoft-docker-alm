#!/usr/bin/env bash

usage()
{
    cat <<END
  dockerGetContainerIP.sh: Este script obtiene las IP de los contenedores que ejecutan la imagen facilitada por parametro
  Parameters:
    -i | --image <docker image> 
      Especifica la imagen que va a buscarse entre los contenedores (p.e. -i azurebrains.valetkey.webapi:latest)
    -h | --help
      Muestra esta ayuda
  Para la correcta ejecucion de este script es necesario tener las herramientas cliente CLI de Docker instaladas y el demonio de Docker corriendo.
  Para mas informacion https://docs.docker.com

END
}

# Definicion de variables
image_id=""

while [[ $# -gt 0 ]]; do
  case "$1" in
    -i | --image )
        image_id="$2"; shift 2 ;;
    -h | --help )
        usage; exit 1 ;;
    *)
        echo "Unknown option $1"
        usage; exit 2 ;;
  esac
done

# Obtenemos el ID del contenedor por el nombre de la imagen
if [[ ! $image_id ]]; then
    echo ''
    echo '  USO INCORRECTO. Debe especificar una imagen'
    echo ''
    usage
    exit 3
fi
container_id=$(docker ps -a -q --filter ancestor=$image_id)

if [[ ! $container_id ]]; then
    echo ''
    echo '  CONTENEDOR NO ENCONTRADO. Debe existir al menos un contenedor ejecutando la imagen especificada'
    echo ''
    usage
    exit 4
fi

# Obtenemos la IP de los contenedores que ejecutan la imagen seleccionada
container_ip=$(docker inspect -f '{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}' $container_id)
echo $container_ip
