# Respaldos de la base de datos

La app respalda la base de datos una vez al día mientras está abierta: revisa
30 segundos después de abrirse y luego cada hora, y respalda si el último
respaldo tiene más de 24 horas. También se puede respaldar con el botón
**Respaldar ahora** en Configuración.

- Cada respaldo es un archivo `ecopapel_AAAA-MM-DD_HHMMSS.dump` en la carpeta
  elegida en Configuración. Sin elegir, es `Documentos\EcoPapel\Respaldos`.
- Se conservan los últimos 30; los más antiguos se borran solos.
- Se crea con `pg_dump` dentro del contenedor de Docker, así que Docker Desktop
  debe estar abierto. Si falla, el motivo aparece en Configuración y un aviso
  en Inicio.
- Conviene elegir una carpeta en otro disco o en una USB: si falla la PC, el
  respaldo sigue a salvo.

## Configuración opcional

En `appsettings.json` o `appsettings.local.json`, sección `Backup`:

| Clave           | Por defecto                | Uso                                         |
|-----------------|----------------------------|---------------------------------------------|
| `Folder`        | `Documentos\EcoPapel\Respaldos` | Carpeta de respaldos (se cambia desde la app) |
| `ContainerName` | `purchase-ticket-postgres` | Nombre del contenedor de PostgreSQL         |
| `DockerPath`    | `docker`                   | Ruta de `docker.exe` si no está en el PATH  |

## Restaurar un respaldo

La restauración no está dentro de la app a propósito: reemplaza todos los
datos. Hazla con la app cerrada, desde PowerShell en la carpeta del proyecto.

1. Carga las variables de `.env` (usuario y base de datos):

   ```powershell
   Get-Content .env | ForEach-Object {
       if ($_ -match '^\s*([^#=]+)=(.*)$') { Set-Item "env:$($matches[1].Trim())" $matches[2].Trim() }
   }
   ```

2. Copia el respaldo al contenedor:

   ```powershell
   docker cp "D:\Respaldos\EcoPapel\ecopapel_2026-10-23_080215.dump" purchase-ticket-postgres:/tmp/respaldo.dump
   ```

3. Primero pruébalo en una base aparte, para revisar que el archivo esté bien:

   ```powershell
   docker exec purchase-ticket-postgres createdb --username $env:POSTGRES_USER prueba_restauracion
   docker exec purchase-ticket-postgres pg_restore --username $env:POSTGRES_USER --dbname prueba_restauracion --no-owner /tmp/respaldo.dump
   ```

4. Si todo está bien, restaura sobre la base real. `--clean` borra lo actual
   antes de cargar el respaldo:

   ```powershell
   docker exec purchase-ticket-postgres pg_restore --username $env:POSTGRES_USER --dbname $env:POSTGRES_DB --clean --if-exists --no-owner /tmp/respaldo.dump
   ```

5. Borra la base de prueba y el archivo copiado:

   ```powershell
   docker exec purchase-ticket-postgres dropdb --username $env:POSTGRES_USER prueba_restauracion
   docker exec purchase-ticket-postgres rm /tmp/respaldo.dump
   ```
