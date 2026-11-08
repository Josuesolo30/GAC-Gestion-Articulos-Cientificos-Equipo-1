# GAC — Gestión de Artículos Científicos

Proyecto académico de Programación III, UNICDA.

## Integrantes

- Josue Rafael Piña Fermin — 2022-30-2-0003
- Urpirio Junior Moreno Vargas — 2022-30-3-0009

## Propósito

Desarrollar una aplicación web para gestionar los artículos científicos
de un grupo de investigación universitario, sus autores, palabras clave,
modalidades de publicación y ubicaciones de copias físicas.

## Tecnologías

- C# y .NET 9.
- ASP.NET Core Web API.
- ASP.NET Core Razor Pages.
- Entity Framework Core.
- ASP.NET Core Identity.

## Organización

- GAC.Domain: entidades y enumeraciones.
  Su carpeta física se llama GAC.Core.
- GAC.Infrastructure: contexto y configuración de persistencia.
- GAC.Application: capa destinada a los casos de uso.
- GAC.Share: contratos compartidos.
- GAC.API: API del sistema.
- GAC.WebUIPages: interfaz con Razor Pages.

## Estado del proyecto

El avance se concentra en las entidades y la configuración del modelo.

La integración con una base de datos, las operaciones del negocio,
las pantallas de gestión y la autenticación operativa están pendientes.

Las clases de Identity y RefreshToken preparan el modelo de acceso;
no representan por sí solas un inicio de sesión implementado.

## Cambios derivados de las observaciones del docente

Verificar en el código cuáles están implementados:

- Grupos de investigación y membresías de investigadores.
- Temas de trabajo de los grupos.
- Colaboración entre investigadores y revistas.
- Catálogo de tipos de informe técnico.
- Relación de las ubicaciones con el grupo que conserva la copia.
- Revisión de publicaciones en congresos y temas de revistas.

## Cómo abrir el proyecto

1. Instalar Visual Studio con la carga de trabajo
   "Desarrollo de ASP.NET y web" y el SDK de .NET 9.
2. Clonar el repositorio.
3. Abrir ProyectoEquipo#1.sln.
4. Restaurar los paquetes NuGet.
5. Compilar la solución.

La API y la web conservan funcionalidades de plantilla.
Todavía no existe un flujo completo para registrar artículos.

## Documentación

Los documentos de requisitos, planificación, backlogs,
diseño.
