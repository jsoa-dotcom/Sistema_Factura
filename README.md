# Sistema de Facturación — RegistroHexagonal

API REST para gestión de facturación (Clientes, Categorías, Productos y Facturas), construida en **visual studio 2022** y **ASP.NET Core Minimal APIs** en **.NET 8** siguiendo **arquitectura hexagonal** (puertos y adaptadores) y cumpliendo las 3 primeras formas normales (1FN, 2FN, 3FN).

## Tabla de contenido

- [Arquitectura](#arquitectura)
- [Estructura del proyecto](#estructura-del-proyecto)
- [Modelo de datos](#modelo-de-datos)
- [Reglas de negocio](#reglas-de-negocio)
- [Tecnologías](#tecnologías)
- [Endpoints](#endpoints)
- [Cómo ejecutar el proyecto](#cómo-ejecutar-el-proyecto)
- [Manejo de errores y logs](#manejo-de-errores-y-logs)
- [Decisiones de diseño](#decisiones-de-diseño)
- [Limitaciones conocidas / mejoras futuras](#limitaciones-conocidas--mejoras-futuras)

## Arquitectura

El proyecto sigue **arquitectura hexagonal** (Ports & Adapters), separando el núcleo de negocio de los detalles de infraestructura y presentación:

- **Domain**: entidades de negocio puras, sin dependencias externas. Contiene también las excepciones de dominio.
- **Application**: casos de uso (`UseCases`), contratos/interfaces (`Ports`) y objetos de transferencia (`DTOs`). Esta capa orquesta la lógica de negocio a través de las interfaces, sin conocer la implementación concreta de persistencia.
- **Infrastructure**: implementación concreta de los repositorios usando **SQLite + Entity Framework Core**, y el adaptador de entrada (endpoints de la API y middleware de errores).

El core de la aplicación (`Application`) nunca depende de `Infrastructure` — la dependencia siempre va hacia adentro, cumpliendo el principio de inversión de dependencias propio de la arquitectura hexagonal.

## Estructura del proyecto

```
RegistroHexagonal/
├── Domain/
│   ├── Exceptions/
│   │   ├── BusinessRuleException.cs
│   │   ├── NotFoundException.cs
│   │   └── NumeroFacturaDuplicadoException.cs
│   ├── Categoria.cs
│   ├── Cliente.cs
│   ├── DetalleFactura.cs
│   ├── Factura.cs
│   └── Producto.cs
├── Application/
│   ├── DTOs/
│   │   ├── ApiResponse.cs
│   │   ├── CategoriaDtos.cs
│   │   ├── ClienteDtos.cs
│   │   ├── DetalleFacturaDtos.cs
│   │   ├── FacturaDtos.cs
│   │   ├── PagedResultDto.cs
│   │   └── ProductoDtos.cs
│   ├── Ports/
│   │   ├── ICategoriaRepository.cs
│   │   ├── ICategoriaService.cs
│   │   ├── IClienteRepository.cs
│   │   ├── IClienteService.cs
│   │   ├── IFacturaRepository.cs
│   │   ├── IFacturaService.cs
│   │   ├── IProductoRepository.cs
│   │   └── IProductoService.cs
│   └── UseCases/
│       ├── CategoriaService.cs
│       ├── ClienteService.cs
│       ├── FacturaService.cs
│       └── ProductoService.cs
├── Infrastructure/
│   ├── Persistence/
│   │   ├── AppDbContext.cs
│   │   ├── SqliteCategoriaRepository.cs
│   │   ├── SqliteClienteRepository.cs
│   │   ├── SqliteFacturaRepository.cs
│   │   └── SqliteProductoRepository.cs
│   └── Presentation/
│       ├── Endpoints/
│       │   ├── CategoriaEndpoints.cs
│       │   ├── ClienteEndpoints.cs
│       │   ├── FacturaEndpoints.cs
│       │   └── ProductoEndpoints.cs
│       └── Middleware/
│           └── ExceptionHandlingMiddleware.cs
├── Migrations/
├── Logs/
├── appsettings.json
├── Program.cs
└── README.md
```

## Modelo de datos

| Tabla | Descripción | PK | FK |
|---|---|---|---|
| **Categorias** | Catálogo de categorías de producto (con borrado lógico) | `Id` (Guid) | — |
| **Productos** | Catálogo de productos (con borrado lógico) | `Id` (Guid) | `CategoriaId` → Categorias |
| **Clientes** | Clientes registrados (con borrado lógico) | `Id` (Guid) | — |
| **Facturas** | Cabecera de cada factura emitida (con anulación lógica) | `Id` (Guid) | `ClienteId` → Clientes |
| **DetallesFactura** | Líneas de producto de cada factura | `Id` (Guid) | `FacturaId` → Facturas, `ProductoId` → Productos |

### Relaciones

- Un **Cliente** puede tener muchas **Facturas** (1:N).
- Una **Categoria** puede tener muchos **Productos** (1:N).
- Una **Factura** tiene muchos **DetalleFactura** (1:N, con borrado en cascada al eliminar la factura).
- Un **Producto** puede aparecer en muchos **DetalleFactura** (1:N).

### Desglose fiscal de la factura

Además del `Subtotal`/`Total` combinados, tanto `DetalleFactura` como `Factura` guardan el desglose separado entre base gravable e IVA, para reflejar correctamente el impuesto tal como se reporta fiscalmente:

- `DetalleFactura.BaseGravable`: precio × cantidad, ya con el descuento efectivo aplicado (línea + factura, en cascada), **sin IVA**.
- `DetalleFactura.ValorIva`: IVA en pesos de esa línea, calculado sobre la base ya descontada.
- `Factura.TotalBaseGravable` / `Factura.TotalIva`: suma de las bases y los IVA de todas las líneas. Se cumple que `TotalBaseGravable + TotalIva = Total`.

## Reglas de negocio

### Precios e IVA

- `PrecioBruto`: precio base del producto, **sin IVA**.
- El IVA y el descuento se manejan como **porcentaje** (ej: `19` = 19%), tanto a nivel de línea (`DetalleFactura.Descuento`) como de factura completa (`Factura.Descuento`).
- **Descuento en cascada**: el descuento de línea y el descuento general de la factura se combinan de forma sucesiva, no sumada:

```
descuentoEfectivo = 1 − (1 − descuentoLínea%) × (1 − descuentoFactura%)
BaseGravable = PrecioBruto × Cantidad × (1 − descuentoEfectivo)
ValorIva = BaseGravable × IVA%
Subtotal = BaseGravable + ValorIva
```

- El `Total` de la factura es la suma directa de los `Subtotal` de todas las líneas — el descuento ya queda aplicado dentro de cada línea, no se vuelve a aplicar al final.

### Datos congelados en el momento de facturar

Para preservar la integridad histórica de una factura ya emitida, se **copian** (no se referencian en vivo) los siguientes datos al momento de crearla:

- `Factura.NombreCliente` y `Factura.DocumentoCliente`.
- `DetalleFactura.NombreProducto`, `PrecioUnitarioBruto` e `Iva`.

Así, si más adelante se edita un `Cliente` o `Producto`, las facturas ya emitidas **no cambian** — reflejan los datos tal como estaban en el momento de la venta.

### Borrado lógico (Activo)

`Cliente`, `Categoria` y `Producto` usan borrado lógico (`Activo` + método `Desactivar()`), en vez de borrado físico. Esto evita:

- Perder historial de ventas al eliminar una entidad referenciada en facturas existentes.
- Bloqueos de integridad referencial (`Restrict`) al intentar borrar algo con historial.

Los registros inactivos pueden consultarse/editarse por `Id` y reactivarse mediante un endpoint dedicado.

### Anulación de factura (Anulada / Motivo)

`Factura` usa un enfoque de baja lógica en vez de eliminación física: cuenta con los campos `Anulada` (bool) y `Motivo` (texto), preservando el documento completo para efectos de auditoría en vez de hacerlo desaparecer del sistema. La anulación se hace mediante `PATCH /facturas/{id}/anular`, no existe un `DELETE` para facturas.

### Numeración de factura

El número de factura (`FAC-0001`, `FAC-0002`...) se **genera automáticamente** en el servidor. Ante una eventual colisión de número por solicitudes simultáneas, el servicio reintenta automáticamente con el siguiente consecutivo.

### Identificación del cliente al facturar

Al crear una factura, se identifica al cliente por su **documento**, no por su `Guid` — la API resuelve internamente el `ClienteId` correspondiente.

### Unicidad del documento del cliente

El documento del cliente tiene un **índice único simple** a nivel de base de datos (no filtrado por estado). Esto significa que, aunque un cliente se desactive, su documento **no puede reutilizarse** en un cliente nuevo mientras el registro desactivado siga existiendo — es una limitación conocida (ver sección de limitaciones).

## Tecnologías

| Paquete | Uso |
|---|---|
| `Microsoft.EntityFrameworkCore.Sqlite` | Proveedor de base de datos SQLite para EF Core |
| `Microsoft.EntityFrameworkCore.Design` | Herramientas de diseño (necesarias para migraciones) |
| `Microsoft.EntityFrameworkCore.Tools` | Comandos `Add-Migration` / `Update-Database` |
| `Swashbuckle.AspNetCore` | Documentación interactiva de la API (Swagger UI) |
| `Serilog.AspNetCore` | Logging estructurado |
| `Serilog.Sinks.File` | Persistencia de logs en archivo (carpeta `Logs/`) |

## Endpoints

Todos los endpoints devuelven respuestas envueltas en `ApiResponse<T>` (`exitoso`, `mensaje`, `datos`), y los listados soportan paginación vía query string (`?pagina=1&tamanoPagina=10`).

### `/categorias`

| Método | Ruta | Descripción |
|---|---|---|
| POST | `/categorias` | Crear categoría |
| GET | `/categorias` | Listar categorías (paginado) |
| GET | `/categorias/{id}` | Obtener por id |
| PUT | `/categorias/{id}` | Actualizar |
| DELETE | `/categorias/{id}` | Desactivar (borrado lógico) |
| PATCH | `/categorias/{id}/reactivar` | Reactivar |

### `/productos`

Mismas operaciones que `/categorias`, además de validar que la `CategoriaId` referenciada exista y esté disponible.

### `/clientes`

Mismas operaciones, con validación de unicidad de `Documento`.

### `/facturas`

| Método | Ruta | Descripción |
|---|---|---|
| POST | `/facturas` | Crear factura (recibe documento del cliente + lista de líneas de producto) |
| GET | `/facturas` | Listar facturas (paginado) |
| GET | `/facturas/{id}` | Obtener por id, con cliente y detalles incluidos |
| PATCH | `/facturas/{id}/anular` | Anular factura indicando un `Motivo` (borrado lógico, sin `DELETE`) |

## Cómo ejecutar el proyecto

1. Restaurar paquetes NuGet (Visual Studio lo hace automático al abrir el proyecto o al compilar).
2. En la Consola del Administrador de Paquetes, generar/aplicar las migraciones:

```
Add-Migration Inicial
Update-Database
```

`Program.cs` también aplica migraciones pendientes automáticamente al arrancar.

3. Ejecutar con **F5**. La base de datos SQLite (`facturacion.db`) se crea automáticamente en la raíz del proyecto.
4. Abrir `/swagger` para probar los endpoints de forma interactiva.

## Manejo de errores y logs

- **`ExceptionHandlingMiddleware`**: middleware global que captura cualquier excepción no controlada, la registra en el log y devuelve una respuesta `ApiResponse` consistente en vez de una página de error genérica.
- **Excepciones de dominio personalizadas** (`Domain/Exceptions/`):
  - `NotFoundException`: recurso no encontrado.
  - `BusinessRuleException`: violación de una regla de negocio (ej: cliente inactivo, categoría inexistente).
  - `NumeroFacturaDuplicadoException`: colisión de número de factura por concurrencia; el servicio reintenta automáticamente antes de propagar el error.
- **Serilog**: registra los logs únicamente en **archivo**, dentro de la carpeta `Logs/` (rotación diaria, retención de 7 días), sin salida por consola.

## Decisiones de diseño

- **`OnDelete(DeleteBehavior.Restrict)`** en las relaciones hacia `Cliente`, `Categoria` y `Producto`: protege la integridad histórica, evitando borrados físicos accidentales de datos con historial de facturación.
- **`OnDelete(DeleteBehavior.Cascade)`** solo entre `Factura` y `DetalleFactura`: un detalle de factura no tiene sentido de forma independiente a su factura.
- **Descuento en cascada** (no sumado): se aplica de forma consistente combinando el descuento de línea y el descuento general de la factura, manteniendo el desglose fiscal (`BaseGravable` + `ValorIva` = `Total`) exacto en cualquier combinación de tasas de IVA.
- **Anulación en vez de borrado físico** para `Factura`: preserva el documento completo, alineado con el mismo criterio de integridad histórica usado en el resto de entidades.
- **Reintento automático ante colisión de consecutivo**: en vez de fallar directamente, `FacturaService.CrearFactura` reintenta con un nuevo número de factura si detecta una violación del índice único (hasta 5 intentos).

## Limitaciones conocidas / mejoras futuras

- No incluye autenticación/autorización.
- No implementa los requisitos de una factura electrónica fiscal real (CUFE, firma digital, formato UBL/XML, envío a un ente de validación) — es un sistema de facturación interno, no una factura electrónica legalmente válida.
- La cadena de conexión está definida directamente en `Program.cs`; se recomienda moverla a `appsettings.json` por ambiente.
