# API de Compuestos Químicos

## Descripción
Este proyecto implementa una API RESTful para gestionar compuestos químicos y sus elementos constituyentes. La API permite realizar operaciones CRUD sobre compuestos químicos, sus elementos, y las relaciones entre ellos, utilizando una arquitectura de capas y el patrón de diseño Repository.

## Tecnologías y Herramientas
- **C# .NET**: Desarrollo de la API y estructura en capas.
- **PostgreSQL 16.x**: Base de datos relacional.
- **Docker**: Contenerización de la base de datos PostgreSQL.
- **Swagger**: Documentación interactiva para probar y verificar las peticiones de la API.
- **Entity Framework (EF)**: Manejo de la conexión y mapeo a la base de datos.

## Estructura del Proyecto
La API está organizada en capas, cada una con una responsabilidad específica:

- **Controller**: Gestión de peticiones HTTP.
  - `CompuestoController.cs`, `ElementoController.cs`
- **Services**: Lógica de negocio y validaciones.
  - `CompuestoService.cs`, `ElementoService.cs`
- **Repository**: Acceso a la base de datos.
  - `CompuestoRepository.cs`, `ElementoRepository.cs`
- **Models**: Definición de las entidades.
  - `Compuesto.cs`, `Elemento.cs`, `Elemento_Por_Compuesto.cs`, `CompuestoSimplificado.cs`, `ElementoSimplificado.cs`
- **DBContext**: Configuración de la conexión a la base de datos PostgreSQL.
  - `PgsqlDbContext.cs`
- **Exceptions**: Excepciones personalizadas.
  - `AppValidationException.cs`, `DbOperationException.cs`
- **Interfaces**: Interfaces de los repositorios.
  - `ICompuestoRepository.cs`, `IElementoRepository.cs`
- **Datos**: Archivos de datos y diagramas de base de datos.
  - Scripts SQL, diagramas y datos de ejemplo en CSV.

## Configuración del Entorno

### Prerrequisitos
1. **Docker**: Asegúrate de tener Docker instalado para correr el contenedor de PostgreSQL.
2. **PostgreSQL**: La base de datos corre en un contenedor PostgreSQL. Utiliza la versión 16.x.


### Configuración de Docker para PostgreSQL
Ejecuta los siguientes comandos para configurar PostgreSQL en Docker:

```bash
docker pull postgres:latest
docker run --name postgres-CompuestosQuimicos -e POSTGRES_PASSWORD=unaClav3 -d -p 5432:5432 postgres:latest
```

### Configuración de la Base de Datos
1. Conéctate al contenedor PostgreSQL usando el siguiente comando:

```bash
docker exec -it postgres-CompuestosQuimicos psql -U postgres
```

2. Ejecuta el script SQL de creación de base de datos que esta ubicado en la carpeta de Datos (CompuestosQuimicos_scriptCreacionModelo_postgresSQL.sql) en el contenedor.

## Archivos y Scripts de la Base de Datos
- **Creación del modelo:** CompuestosQuimicos_scriptCreacionModelo_postgresSQL.sql
- **Diagrama relacional:** CompuestoQuimico_diagramaRelacional_2024.png
- **Datos de ejemplo:** CompuestosQuimicos_datos_elementos.csv, CompuestoQuimicos_datos_compuestos.csv

## Configuración de la API en C#

1. **Clonar el repositorio** (https://github.com/EmanuelR521/tadb_202420_ex03.git)
2. **Configurar conexión a PostgreSQL:** Edita appsettings.json en la raíz del proyecto e ingresa las credenciales de la base de datos:

```bash
{
  "ConnectionStrings": {
    "CompuestoQuimico": "Server=localhost;Port=5432;Username=quimico_usr;Password=unaClav3;Database=compuestosquimicos_db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```
3. **Iniciar la API**
```bash
dotnet build
dotnet run
```

## Endpoints de la API

### Elementos
- **GET /api/elementos**: Listar todos los elementos.
- **GET /api/elementos/{id}**: Obtener un elemento por ID.
- **POST /api/elementos**: Crear un nuevo elemento.
- **PUT /api/elementos/{id}**: Actualizar un elemento.
- **DELETE /api/elementos/{id}**: Eliminar un elemento.

### Compuestos
- **GET /api/compuestos**: Listar todos los compuestos.
- **GET /api/compuestos/{id}**: Obtener un compuesto por ID, incluyendo sus elementos.
- **POST /api/compuestos**: Crear un nuevo compuesto con sus elementos.
- **PUT /api/compuestos/{id}**: Actualizar un compuesto y sus elementos.
- **DELETE /api/compuestos/{id}**: Eliminar un compuesto

### Documentación y Pruebas con Swagger
Al iniciar la API, Swagger se encuentra disponible en:
```bash
https://localhost:<puerto>/swagger
```
Swagger permite explorar y probar todos los endpoints de la API, ver los parámetros de entrada y salida, y probar las respuestas.

## Modelo de Datos
El modelo de datos incluye las siguientes entidades principales:

- **Elementos**: Información de cada elemento químico.
- **Compuestos**: Detalles de cada compuesto químico.
- **Elementos por Compuesto**: Relación de elementos y compuestos.

## Notas Adicionales
- **Pruebas**: Los datos de prueba incluyen tres compuestos (en estado líquido, sólido, y gaseoso) para validar el correcto funcionamiento de la API.
- **Control de Errores**: La API implementa excepciones personalizadas (`AppValidationException`, `DbOperationException`) para el control de errores y manejo de validaciones.

## Contacto
- **Juan Dario Rodas** - [juand.rodasm@upb.edu.co](mailto:juand.rodasm@upb.edu.co)
- **Carlos Andres Sanabria** - [carlos.sanabria@upb.edu.co](mailto:carlos.sanabria@upb.edu.co)
- **Emanuel Rios Bolivar** - [emanuel.riosb@upb.edu.co](mailto:emanuel.riosb@upb.edu.co)





