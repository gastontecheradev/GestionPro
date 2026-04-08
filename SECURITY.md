# Seguridad — GestionPro

## Datos confidenciales protegidos

Este proyecto usa **User Secrets** de .NET para manejar datos sensibles.
Los siguientes datos **NUNCA** se suben al repositorio:

### Base de datos
- El archivo `GestionPro.db` (SQLite) está en `.gitignore`
- Se genera automáticamente al ejecutar la aplicación con las migraciones

### Credenciales del administrador
- Las credenciales del seed se leen de User Secrets en desarrollo
- En producción, se configuran mediante variables de entorno

### Cómo configurar User Secrets

```bash
# Navegar al proyecto
cd GestionPro/GestionPro

# Configurar credenciales del admin
dotnet user-secrets set "SeedAdmin:Email" "admin@tudominio.com"
dotnet user-secrets set "SeedAdmin:Password" "TuPasswordSeguro123!"

# Ver los secrets configurados
dotnet user-secrets list

# Eliminar un secret
dotnet user-secrets remove "SeedAdmin:Password"
```

### Para producción (somee.com u otro hosting)

Configurá las variables de entorno en el panel del hosting:

```
SeedAdmin__Email=admin@tudominio.com
SeedAdmin__Password=PasswordSeguroProduccion!
ConnectionStrings__DefaultConnection=Data Source=GestionPro.db
```

> Nota: En variables de entorno, los `:` se reemplazan por `__` (doble guion bajo).

## Archivos excluidos por .gitignore

| Archivo/Patrón | Razón |
|---------------|-------|
| `*.db`, `*.db-shm`, `*.db-wal` | Base de datos SQLite |
| `appsettings.Production.json` | Configuración de producción |
| `launchSettings.json` | Puede contener URLs locales |
| `.vs/`, `*.user` | Configuración personal de Visual Studio |
