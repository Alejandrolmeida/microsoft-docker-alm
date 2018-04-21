Desde el administrador de paquetes y con el proyecto que tiene EF como proyecto de inicio. 

Con la base de datos creada, podemos lanzar el siguiente comando para crear las tablas:

# Genera los script de cambios
$ add-migrations 

# Ejecuta el script de cambios contra la base de datos
$ update-database
