# Guia tecnica de la Liga Arnaldo Chacon Soto

Esta guia explica como esta organizada la aplicacion, como viaja la informacion entre las pantallas y SQL Server, y que funciones estan disponibles actualmente.

## 1. Resumen del sistema

La aplicacion usa ASP.NET Web Forms sobre .NET Framework 4.7.2. AdminLTE proporciona el diseno visual y SQL Server guarda equipos, competencias, jornadas, partidos y usuarios. Las paginas publicas permiten consultar la liga sin iniciar sesion. Las paginas administrativas requieren una cuenta.

```text
Navegador (pagina .aspx)
        | envia una solicitud / postback
        v
Code-behind (.aspx.cs) valida la entrada y decide que hacer
        | llama a un repositorio
        v
Datos/*.cs construye consultas SQL parametrizadas
        | ConexionBD obtiene la cadena de Web.config
        v
SQL Server -> devuelve filas o confirma un cambio
        | DataTable / resultado
        v
Code-behind enlaza datos a controles y Web Forms genera HTML
```

## 2. Como leer los archivos

- `Vistas/*.aspx`: estructura HTML y controles Web Forms (formularios, botones, tablas y repetidores). La pagina suele usar `Plantilla.Master`.
- `Vistas/*.aspx.cs`: code-behind. Atiende la carga inicial, los clics y los postbacks; valida valores y llama a los repositorios.
- `Vistas/*.aspx.designer.cs`: campos que conectan los controles declarados en el `.aspx` con el code-behind. Visual Studio lo mantiene.
- `Vistas/Plantilla.Master`: marco compartido: menu, encabezado, pie y referencias a CSS/JS de AdminLTE.
- `Datos/*Repository.cs`: consultas y cambios a la base de datos. Mantiene el SQL fuera de la presentacion.
- `Datos/ConexionBD.cs`: lee la cadena `LigaArnaldoChacon` desde `Web.config` y crea conexiones.
- `Logica/`: objetos y reglas compartidas, como la cuenta de usuario y las comprobaciones de permisos.
- `Datos/Crear_BaseDatos_LigaArnaldoChacon.sql`: estructura inicial de tablas, relaciones, datos de equipos/categorias y vistas de calculo.
- `App_Data/Campeones.json`: archivo donde se conserva el archivo historico de campeones que administra la vista de campeones.

## 3. Recorrido de una consulta publica

Al entrar a `Default.aspx`, la aplicacion redirige a `Vistas/Home.aspx`. La pagina llama a `LigaRepository` para obtener equipos, tabla, partidos pendientes y resultados. Cada consulta incluye la categoria seleccionada y los partidos/resultados se filtran por estado. Los resultados se reciben en un `DataTable`; el code-behind lo asigna a un `Repeater` o control equivalente. Web Forms convierte esos datos en HTML.

Las vistas publicas principales son:

- `Vistas/Home.aspx`: resumen, tabla y partidos por categoria.
- `Vistas/Equipos.aspx`: equipos activos y categoria.
- `Vistas/Posiciones.aspx`: tabla calculada.
- `Vistas/Campeones.aspx`: campeones guardados en el archivo JSON.

## 4. Recorrido para programar o corregir un partido

1. El administrador inicia sesion y abre `Vistas/AdministrarPartidos.aspx`.
2. La pagina comprueba el rol y carga competencias. Al abrir una competencia se asegura que existan las jornadas del 1 al 9.
3. Los equipos del selector son las inscripciones activas de esa competencia.
4. Al guardar, el code-behind valida los campos y llama a `AdministracionRepository.CrearPartido`. El repositorio vuelve a validar la cancha y ejecuta un `INSERT` parametrizado con estado `PENDIENTE`.
5. Para corregir la programacion, se carga el partido y se guarda su `VersionFila`. El `UPDATE` solo funciona si el partido sigue pendiente y la version no cambio desde que se cargo.
6. Para eliminar, solo se permite eliminar un partido pendiente y con la misma version cargada. Si otro usuario ya lo modifico, la operacion falla y la pantalla debe recargarse para trabajar con el dato actual.
7. Al ingresar el marcador, se guardan goles local/visita y el estado pasa a `FINALIZADO`.
8. Las consultas publicas muestran los partidos pendientes como proximos y los finalizados como resultados. La vista SQL de posiciones calcula el acumulado a partir de partidos finalizados.

La cancha se exige tanto en la pantalla como en el servidor; el valor habitual es `Estadio Alex Pineda Chacon`. La validacion del servidor es la que protege los datos aunque el navegador no ejecute JavaScript.

## 5. Como se calcula la tabla

La vista SQL `dbo.vw_TablaPosiciones` resume los partidos finalizados por equipo: PJ, PG, PE, PP, GF, GC, DG y PTS. La puntuacion es 3 por victoria, 1 por empate y 0 por derrota. La consulta de posiciones ordena por puntos, diferencia de goles y goles a favor, de mayor a menor. Al guardar/corregir un resultado, el calculo se actualiza al consultar la vista; no se mantiene una tabla de posiciones manual.

## 6. Inicio de sesion y permisos

- `Vistas/Acceso.aspx`: autenticacion, cierre de sesion y configuracion inicial del primer superadministrador.
- El primer superadministrador solo se puede crear desde el mismo equipo que hospeda el sitio (`Request.IsLocal`) y mientras la tabla de usuarios este vacia.
- `UsuarioRepository` guarda contrasenas como hash PBKDF2-SHA256 con sal aleatoria; nunca guarda la contrasena legible.
- `Logica/SeguridadWeb.cs` protege las paginas administrativas y revisa que el rol sea Administrador.
- Superadministrador: administra cuentas y tiene acceso a las operaciones del torneo.
- Administrador: gestiona equipos, partidos, resultados y sanciones.
- Usuario regular y visitantes: consultan la informacion publica. No pueden ejecutar acciones administrativas.
- La sesion se conserva en el servidor; al cerrar sesion se limpia.

Para configurar el primer superadministrador: ejecutar la aplicacion en la computadora anfitriona, abrir `Vistas/Acceso.aspx` desde esa misma computadora y completar el formulario inicial. Luego iniciar sesion y crear administradores/usuarios desde `Vistas/AdministrarUsuarios.aspx`.

## 7. Base de datos y configuracion

La cadena de conexion esta en `Web.config` bajo `connectionStrings`, con el nombre `LigaArnaldoChacon`. `ConexionBD.CrearConexion()` la lee por nombre. Si se cambia el servidor o la base de datos, se edita esa entrada y se mantiene el mismo nombre. No copies la cadena con credenciales a capturas, repositorios publicos ni mensajes; usa una cuenta SQL con permisos minimos necesarios.

El script SQL crea el esquema relacional y vistas. Las entidades centrales son `Temporadas`, `Categorias`, `Competencias`, `Equipos`, `EquipoInscripciones`, `Jornadas` y `Partidos`. `Partidos` apunta a una competencia, jornada y dos inscripciones de equipo, lo cual conserva la categoria/temporada de cada encuentro. `Usuarios` guarda cuentas y rol.

Para revisar datos desde SQL Server Management Studio, conecta a la misma instancia configurada y selecciona la base que indica la cadena. Consultas utiles:

```sql
SELECT * FROM dbo.Competencias;
SELECT * FROM dbo.Partidos ORDER BY FechaHora;
SELECT * FROM dbo.vw_TablaPosiciones ORDER BY PTS DESC, DG DESC, GF DESC;
SELECT Estado, COUNT(*) AS Cantidad FROM dbo.Partidos GROUP BY Estado;
```

## 8. Uso desde varias computadoras

Todos los navegadores deben abrir la misma direccion del servidor web; la aplicacion consulta la misma base central. No se sincronizan bases locales de cada equipo. El control `VersionFila` ayuda a detectar ediciones simultaneas de un partido: la primera guarda el cambio y una segunda edicion con una version anterior no sobrescribe silenciosamente el dato nuevo.

## 9. Funciones disponibles y pendientes

Conectado actualmente: consulta publica de equipos, posiciones, partidos pendientes y resultados; filtro por categoria; acceso por roles; administracion de cuentas por superadministrador; alta de equipos con escudo seleccionado o arrastrado desde el dispositivo; registro de jugadores en sus plantillas; programacion, edicion y eliminacion restringida de partidos pendientes; captura/correccion de marcadores; registro de tarjetas amarillas y rojas; alineaciones por partido con titulares/suplentes y consulta publica; archivo de campeones, portero y goleador.

Pendiente de interfaz o conexion completa: editar/desactivar equipos y jugadores; registrar goles individuales, lesiones y check-in; tesoreria; exportar listados a PDF; crear temporadas/competencias desde una pantalla; calculo automatico del premio individual al portero. La pantalla de equipos permite altas de clubes y jugadores y muestra las plantillas. Aunque el script SQL contiene tablas/vistas para algunas estadisticas (por ejemplo goles y porteros), una tabla o vista SQL por si sola no significa que exista una pantalla para capturar esos datos. El archivo de campeones se guarda en JSON y no en SQL.

Para una base creada con una version anterior, ejecutar en SQL Server Management Studio el archivo `Datos/Actualizar_Roles_LigaArnaldoChacon.sql`. El script amplia `Usuarios.Rol`, deja los roles Superadministrador/Administrador/Usuario y promueve el administrador mas antiguo a superadministrador si todavia no existe uno. La tabla `EventosPartido` ya soporta AMARILLA y ROJA; no requiere otra tabla para guardar sanciones. La tabla `PartidoAlineaciones` ya guarda el tipo TITULAR/SUPLENTE por jugador y partido; no requiere otra migracion. Las tarjetas y alineaciones requieren que el jugador ya pertenezca a la plantilla del equipo; se puede dar de alta desde `Vistas/AdministrarEquipos.aspx`.

El torneo acordado es Liga Mayor con 10 equipos, 9 jornadas y 18 partidos por equipo bajo formato ida y vuelta. La programacion de partidos se carga manualmente; la pagina asegura las nueve jornadas, pero no genera automaticamente los 90 cruces.

## 10. Archivos recomendados para seguir el flujo

1. `Vistas/Home.aspx.cs` y `Datos/LigaRepository.cs`: lectura publica.
2. `Vistas/AdministrarPartidos.aspx.cs` y `Datos/AdministracionRepository.cs`: operaciones de partidos.
3. `Vistas/Acceso.aspx.cs`, `Datos/UsuarioRepository.cs` y `Logica/SeguridadWeb.cs`: autenticacion y permisos.
4. `Datos/Crear_BaseDatos_LigaArnaldoChacon.sql`: tablas, relaciones, reglas y vistas calculadas.
5. `Web.config` y `Datos/ConexionBD.cs`: conexion a SQL Server.
