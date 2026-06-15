# SGIP — Sistema de Gestión de Inversiones y Préstamos

SGIP es una aplicación web fullstack para la gestión de préstamos financieros. Permite a los usuarios simular préstamos, ver el cronograma de pagos completo antes de comprometerse, solicitar préstamos que se aprueban automáticamente según reglas de negocio, y registrar transacciones con garantías de idempotencia para evitar cobros duplicados.

---

## Links de despliegue

| Servicio | URL |
|---|---|
| Frontend | _pendiente de despliegue_ |
| Backend (Swagger) | _pendiente de despliegue_ |

**Credenciales de prueba:** No se requiere autenticación. Todas las operaciones usan un `userId` fijo (`user-hardcoded-002`) que representa al usuario activo.

La base de datos viene con dos préstamos precargados:
- **$5,000** a 12 meses — estado `Active`
- **$15,000** a 24 meses — estado `Pending`

---

## Tecnologías utilizadas

### Backend
- **.NET 8** — framework principal de la API
- **PostgreSQL** — base de datos relacional
- **Entity Framework Core** — ORM con migraciones versionadas
- **Swashbuckle (Swagger/OpenAPI)** — documentación de la API

### Frontend
- **Next.js 14** (App Router) — framework de React con soporte para server components
- **TypeScript** — tipado estático
- **Tailwind CSS** — estilos utilitarios
- **React Hook Form + Zod** — manejo y validación de formularios

### Testing
- **TestContainers** — para tests de integración con base de datos real
- **xUnit** — framework de pruebas unitarias
- **Moq** — mocking de dependencias

### Decisiones técnicas importantes

**Clean Architecture:** En este caso se optó por implementar el patrón Command/Query directamente mediante interfaces (`ICommandHandler<TCommand, TResponse>`),
manteniendo el proyecto bien estructurado, cada caso de uso es un archivo con su comando y su handler, y el registro en DI es explícito y legible. 
El trade-off es que hay más código manual, pero a la escala de este proyecto eso es preferible a agregar una dependencia extra.

**Result pattern en lugar de excepciones:** Los handlers nunca lanzan excepciones para errores de negocio. 
En su lugar retornan `Result<T>` con un `Error` tipado. Los controladores usan `.Match(onSuccess, onFailure)` para convertir eso en un `ActionResult`. 
Esto hace que el flujo de errores sea explícito y fácil de testear sin necesidad de `try/catch`. Ademas, lanzar excepciones para validaciones puede afectar directamente en el performance.

**Proyecciones en repositorios:** Los métodos de consulta en `ILoanRepository` e `ITransactionRepository` aceptan un `Expression<Func<TEntity, TResult>> selector`. 
Esto evita cargar grafos completos de entidades cuando solo se necesitan unos pocos campos, sin tener que duplicar lógica de filtrado fuera del repositorio.

---

## Instalación local

### Prerrequisitos

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### Levantar el proyecto

El siguiente comando levanta los tres servicios juntos — frontend, API y base de datos:

```bash
docker-compose up --build
```

| Servicio | URL |
|---|---|
| Frontend (Next.js) | `http://localhost:3000` |
| Backend (API) | `http://localhost:8080` |
| Swagger | `http://localhost:8080/swagger` |
| PostgreSQL | `localhost:5432` |

Docker se encarga de construir las imágenes, levantar PostgreSQL, aplicar las migraciones y cargar el seed data inicial. No es necesario tener .NET ni Node.js instalados localmente.

Para detenerlo:

```bash
docker-compose down
```

---

## Variables de entorno

### Backend

| Variable | Descripción | Ejemplo |
|---|---|---|
| `DATABASE_URL` | Connection string en formato PostgreSQL URL (usado en producción/Railway) | `postgresql://user:pass@host:5432/db` |
| `ConnectionStrings__DefaultConnection` | Alternativa en formato Npgsql (usado en desarrollo sin Docker) | `Host=localhost;Database=sgip;...` |

En desarrollo con Docker no necesitas configurar nada — las variables ya están definidas en `docker-compose.yml`.

### Frontend

Crear un archivo `.env.local` en la carpeta `frontend/`:

```env
NEXT_PUBLIC_API_URL=http://localhost:8080
```

---

## Testing

Para los test de integracion, se utiliza `TestContainers` para levantar una instancia real de PostgreSQL durante la ejecución de los tests. 
Esto permite validar la integración completa entre los handlers, repositorios y la base de datos sin depender de mocks.

```bash
# Desde la raíz del repositorio
dotnet test FinTech.slnx

# Correr solo una clase de tests
dotnet test --filter "FullyQualifiedName~ApplyForLoanCommandHandlerTests"
```

Los tests son unitarios y no requieren base de datos. Cada handler se prueba directamente instanciándolo con repositorios mockeados. Los casos cubiertos incluyen:

- Cálculo correcto de cuota fija (sistema francés)
- Auto-aprobación cuando el monto es < $10,000 y hay menos de 2 préstamos activos
- Rechazo cuando se excede el límite de préstamos activos
- Rechazo cuando la cuota supera el 40% del ingreso mensual declarado
- Validaciones de monto mínimo y máximo

---

## Arquitectura

El proyecto sigue **Clean Architecture** dividido en cinco capas con dependencias unidireccionales:

```
FinTech.API  →  FinTech.Application  →  FinTech.Domain  ←  FinTech.Persistence
                                              ↑
                                        SharedKernel
```

- **FinTech.Domain** (`src/Core/FinTech.Domain`): entidades (`Loan`, `Transaction`, `PaymentSchedule`), enums y contratos de repositorios.
- **FinTech.Application** (`src/Core/FinTech.Application`): casos de uso implementados como handlers CQRS, DTOs y mappers.
- **FinTech.Persistence** (`src/FinTech.Persistence`): implementaciones de repositorios con EF Core, configuraciones de entidades y seed data.
- **FinTech.API** (`src/FinTech.API`): controladores REST, configuración de startup y extensiones.
- **SharedKernel** (`src/SharedKernel`): tipos compartidos entre capas — `Result<T>`, `Error`, `IUnitOfWork`, `FinancialCalculator` y `FinancialConstants`.

### Patrones implementados

**Repository Pattern** — cada entidad tiene su interfaz de repositorio en el dominio (`ILoanRepository`, `ITransactionRepository`) con su implementación en Persistence. Esto permite testear los handlers sin base de datos real.

**Command/Query Pattern** — las operaciones de escritura son `ICommand` y las de lectura son `IQuery`, cada una con su handler correspondiente. 

**Result Pattern** — reemplaza el uso de excepciones para errores de negocio, haciendo el flujo de errores explícito y predecible.

---

## Decisiones de diseño

**¿Por qué React Hook Form + Zod y no Formik?**
React Hook Form tiene mejor rendimiento porque no re-renderiza el formulario completo en cada cambio de campo — solo actualiza el campo modificado. Zod permite definir el schema de validación una sola vez y reutilizarlo tanto en el cliente como potencialmente en el servidor.

**¿Por qué Next.js App Router con `force-dynamic`?**
Las páginas de listados (`/loans`, `/transactions`) usan `force-dynamic` para evitar que Next.js cachee los datos en build time. Dado que el contenido cambia frecuentemente, el caché estático causaría que el usuario vea datos desactualizados.

**¿Qué se simplificó?**
- La TEA está fija en 24% para todos los préstamos. En producción esto variaría según el perfil de riesgo del cliente.
- El `userId` está hardcodeado. Se dejó así porque la prueba no requería autenticación y agregar auth hubiera consumido tiempo que se invirtió en la lógica de negocio.
- Solo se implementó el sistema de cuota fija (francés). El sistema alemán queda como mejora futura.

---

## Supuestos y limitaciones

### Funcionalidades no implementadas
- Autenticación y autorización de usuarios
- Sistema de cuota decreciente (sistema alemán)
- CI/CD

### Simplificaciones realizadas
- Un único usuario hardcodeado (`user-hardcoded-002`) en lugar de un sistema de sesiones
- TEA fija al 24% sin variación por perfil de riesgo
- El estado `Active` no se asigna automáticamente al aprobar — queda en `Approved` hasta que se implemente el flujo de desembolso completo

### Mejoras futuras
- Autenticación con JWT
- Notificaciones de vencimiento de cuotas
- Dashboard con métricas de cartera (monto total prestado, morosidad, etc.)
- Sistema de cuota decreciente (sistema alemán)
- Tests de integración con base de datos real usando Testcontainers
