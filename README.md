# 🏥 HealthCare API

Proyecto basado en **Clean Architecture**, diseñado para mantener una estructura escalable, mantenible y desacoplada.

---

# 🧠 ¿Qué es Clean Architecture?

Clean Architecture es un enfoque de diseño que separa el sistema en capas independientes, donde:

- Las reglas de negocio no dependen de frameworks
- Las dependencias siempre apuntan hacia el núcleo (Domain)
- Se facilita el mantenimiento, testing y escalabilidad

---

# 🧱 Estructura del Proyecto

## 📦 HealthCare.Api
👉 **Capa de presentación (entry point)**

- Contiene los **controllers**
- Configuración de la aplicación (JWT, CORS, middlewares)
- Maneja las peticiones HTTP

---

## 📦 HealthCare.Application
👉 **Capa de lógica de aplicación**

- Casos de uso (Use Cases)
- Servicios de aplicación
- Validaciones
- Orquestación de operaciones

💡 No contiene lógica de acceso a datos ni dependencias externas

---

## 📦 HealthCare.Domain
👉 **Capa central (núcleo del sistema)**

- Entidades del dominio
- Interfaces (contratos)
- Reglas de negocio

💥 Esta capa NO depende de ninguna otra

---

## 📦 HealthCare.Infrastructure
👉 **Capa de acceso a datos y servicios externos**

- Implementación de repositorios
- Conexión a base de datos (Dapper, EF, etc.)
- Integraciones externas

💡 Depende de Domain, pero Domain no depende de ella

---

## 📦 HealthCare.Shared
👉 **Capa de utilidades compartidas**

- Clases comunes
- Helpers
- Constantes
- Modelos reutilizables

---

# 🔄 Flujo de Dependencias

```text
Api → Application → Domain
          ↓
   Infrastructure