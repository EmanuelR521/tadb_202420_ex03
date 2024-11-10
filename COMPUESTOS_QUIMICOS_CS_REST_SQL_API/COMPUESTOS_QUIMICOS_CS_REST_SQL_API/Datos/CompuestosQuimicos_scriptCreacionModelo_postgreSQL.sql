-- Curso de Tópicos Avanzados de Base de Datos - UPB 202420
-- Juan Dario Rodas - juand.rodasm@upb.edu.co

-- Proyecto: Compuestos Quimicos
-- Motor de Base de datos: PostgreSQL 16.x

-- integrantes
-- carlos andres sanabria - carlos.sanabria@upb.edu.co - ID: 000493912
-- emanuel rios bolivar - emanuel.riosb@upb.edu.co - ID: 000491739

-- ***********************************
-- Configuración de PostgreSQL en Docker
-- ***********************************

-- Descargar la imagen
docker pull postgres:latest

-- Crear el contenedor
docker run --name postgres-CompuestosQuimicos -e POSTGRES_PASSWORD=unaClav3 -d -p 5432:5432 postgres:latest

-- ****************************************
-- Creación de la base de datos y usuarios
-- ****************************************

-- Con usuario postgres:
-- Crear la base de datos
create database compuestosquimicos_db;

-- Conectarse a la base de datos
\c compuestosquimicos_db;

-- Creamos un esquema para almacenar todo el modelo de datos del dominio
-- esquema principal
create schema core;

-- crear el usuario con el que se implementará la creación del modelo
create user quimico_app with encrypted password 'unaClav3';

-- asignación de privilegios para el usuario
grant connect on database compuestosQuimicos_db to quimico_app;
grant create on database compuestosQuimicos_db to quimico_app;
grant create, usage on schema core to quimico_app;
alter user quimico_app set search_path to core;

-- crear el usuario con el que se conectará la aplicación
create user quimico_usr with encrypted password 'unaClav3';

-- asignación de privilegios para el usuario
grant connect on database compuestosQuimicos_db to quimico_usr;
grant usage on schema core to quimico_usr;
alter default privileges for user quimico_app in schema core grant insert, update, delete, select on tables to quimico_usr;
alter default privileges for user quimico_app in schema core grant execute on routines to quimico_usr;
alter user quimico_usr set search_path to core;

-- Activar la extensión que permite el uso de UUID
create extension if not exists "uuid-ossp";

-- ****************************************
-- Creación de Tablas Principales
-- ****************************************

-- Tabla de Elementos
create table core.elementos (
    id                  integer generated always as identity primary key,
    nombre              varchar(50) not null,
    simbolo             varchar(5) not null,
    numero_atomico      integer not null,
    config_electronica  varchar(50) not null,
    elemento_uuid       uuid default gen_random_uuid() unique,
    constraint nombre_simbolo_uk unique (nombre, simbolo)
);

comment on table core.elementos is 'Tabla que almacena los elementos químicos';
comment on column core.elementos.id is 'ID del elemento';
comment on column core.elementos.nombre is 'Nombre completo del elemento';
comment on column core.elementos.simbolo is 'Símbolo químico del elemento';
comment on column core.elementos.numero_atomico is 'Número atómico del elemento';
comment on column core.elementos.config_electronica is 'Configuración electrónica del elemento';
comment on column core.elementos.elemento_uuid is 'UUID del elemento para uso en la API';

-- Tabla de Compuestos
create table core.compuestos (
    id                  integer generated always as identity primary key,
    nombre              varchar(30) not null,
    formula_quimica     varchar(30) not null,
    masa_molar          numeric(6,3) not null,
    estado_agregacion   varchar(30) not null,
    compuesto_uuid      uuid default gen_random_uuid() unique,
    constraint nombre_formula_quimica_uk unique (nombre, formula_quimica)
);

comment on table core.compuestos is 'Tabla que almacena los compuestos';
comment on column core.compuestos.id is 'ID del compuesto';
comment on column core.compuestos.nombre is 'Nombre completo del compuesto';
comment on column core.compuestos.formula_quimica is 'Fórmula química del compuesto';
comment on column core.compuestos.masa_molar is 'Masa molar del compuesto en gramos/mol';
comment on column core.compuestos.estado_agregacion is 'Estado de agregación del compuesto (Ej: sólido, líquido, gaseoso)';
comment on column core.compuestos.compuesto_uuid is 'UUID del compuesto para uso en la API';

-- Tabla de elementos por compuestos
create table core.elementos_por_compuestos (
    elemento_uuid uuid not null constraint elementos_fk references core.elementos(elemento_uuid),
    compuesto_uuid uuid not null constraint compuestos_fk references core.compuestos(compuesto_uuid),
    cantidad_atomos integer not null,
    primary key (elemento_uuid, compuesto_uuid)
);

comment on table core.elementos_por_compuestos is 'Relación de cantidad de átomos de cada elemento en un compuesto';
comment on column core.elementos_por_compuestos.elemento_uuid is 'UUID del elemento en el compuesto';
comment on column core.elementos_por_compuestos.compuesto_uuid is 'UUID del compuesto';
comment on column core.elementos_por_compuestos.cantidad_atomos is 'Cantidad de átomos del elemento en el compuesto';


-- *****************************
-- Nota: insertar datos de CSV
-- *****************************






----------------------------Pasos Siguientes---------------------------------

-- ****************************************
-- Creación de Procedimientos Almacenados para CRUD
-- ****************************************


-- Procedimiento para Insertar un Nuevo Elemento
create or replace procedure core.p_insertar_elemento(
    in p_nombre text,
    in p_simbolo text,
    in p_numero_atomico int,
    in p_config_electronica text
)
language plpgsql as $$

declare
    l_total_registros integer;

begin
    -- Validación de campos nulos o vacíos
    if p_nombre is null or
       p_simbolo is null or
       p_numero_atomico is null or
       p_config_electronica is null or
       length(p_nombre) = 0 or
       length(p_simbolo) = 0 or
       length(p_config_electronica) = 0 then
            raise exception 'El nombre, símbolo, número atómico y configuración electrónica no pueden ser nulos o vacíos';
    end if;

    -- Validación de unicidad para nombre y símbolo
    select count(*)
    into l_total_registros
    from core.elementos
    where lower(nombre) = lower(p_nombre) or lower(simbolo) = lower(p_simbolo);

    if l_total_registros > 0 then
        raise exception 'Ya existe un elemento con ese nombre o símbolo';
    end if;

    -- Inserción del nuevo elemento
    insert into core.elementos (nombre, simbolo, numero_atomico, config_electronica)
    values (initcap(p_nombre), upper(p_simbolo), p_numero_atomico, p_config_electronica);

end;
$$;



-- Procedimiento para Actualizar un Elemento Existente
create or replace procedure core.p_actualizar_elemento(
    in p_elemento_uuid uuid,
    in p_nombre text,
    in p_simbolo text,
    in p_numero_atomico int,
    in p_config_electronica text
)
language plpgsql as $$

declare
    l_total_registros integer;

begin
    -- Validación de existencia del elemento con el UUID
    select count(*) into l_total_registros
    from core.elementos
    where elemento_uuid = p_elemento_uuid;

    if l_total_registros = 0 then
        raise exception 'No existe un elemento registrado con ese UUID';
    end if;

    -- Validación de campos nulos o vacíos
    if p_nombre is null or
       p_simbolo is null or
       p_numero_atomico is null or
       p_config_electronica is null or
       length(p_nombre) = 0 or
       length(p_simbolo) = 0 or
       length(p_config_electronica) = 0 then
            raise exception 'El nombre, símbolo, número atómico y configuración electrónica no pueden ser nulos o vacíos';
    end if;

    -- Validación de unicidad para nombre y símbolo
    select count(*) into l_total_registros
    from core.elementos
    where (lower(nombre) = lower(p_nombre) or lower(simbolo) = lower(p_simbolo))
    and elemento_uuid != p_elemento_uuid;

    if l_total_registros > 0 then
        raise exception 'Ya existe un elemento con ese nombre o símbolo';
    end if;

    -- Actualización del elemento con el UUID
    update core.elementos
    set nombre = initcap(p_nombre),
        simbolo = upper(p_simbolo),
        numero_atomico = p_numero_atomico,
        config_electronica = p_config_electronica
    where elemento_uuid = p_elemento_uuid;

end; $$;



-- Procedimiento para Eliminar un Elemento
create or replace procedure core.p_eliminar_elemento(
    in p_elemento_uuid uuid
)
language plpgsql as $$

declare
    l_total_registros integer;
begin
    select count(*) into l_total_registros
    from core.elementos
    where elemento_uuid = p_elemento_uuid;

    if l_total_registros = 0 then
        raise exception 'No existe un elemento registrado con ese UUID';
    end if;

    -- Eliminación del elemento
    delete from core.elementos
    where elemento_uuid = p_elemento_uuid;

end; $$;



-- Procedimiento para Insertar un Nuevo Compuesto
create or replace procedure core.p_insertar_compuesto(
    in p_nombre text,
    in p_formula_quimica text,
    in p_masa_molar numeric,
    in p_estado_agregacion text
)
language plpgsql as $$

begin
    -- Validación de campos nulos o vacíos
    if p_nombre is null or p_formula_quimica is null then
        raise exception 'El nombre y la fórmula química no pueden ser nulos';
    end if;

    -- Inserción del nuevo compuesto
    insert into core.compuestos (nombre, formula_quimica, masa_molar, estado_agregacion)
    values (initcap(p_nombre), p_formula_quimica, p_masa_molar, initcap(p_estado_agregacion));

end; $$;



-- Procedimiento para Actualizar un Compuesto
create or replace procedure core.p_actualizar_compuesto(
    in p_compuesto_uuid uuid,
    in p_nombre text,
    in p_formula_quimica text,
    in p_masa_molar numeric,
    in p_estado_agregacion text
)
language plpgsql as $$

declare
    l_total_registros integer;

begin
    -- Verificación de existencia del compuesto
    select count(*) into l_total_registros from core.compuestos where compuesto_uuid = p_compuesto_uuid;
    if l_total_registros = 0 then
        raise exception 'No existe un compuesto registrado con ese UUID';
    end if;

    -- Actualización del compuesto
    update core.compuestos
    set nombre = initcap(p_nombre),
        formula_quimica = p_formula_quimica,
        masa_molar = p_masa_molar,
        estado_agregacion = initcap(p_estado_agregacion)
    where compuesto_uuid = p_compuesto_uuid;
end; $$;



-- Procedimiento para Eliminar un Compuesto
create or replace procedure core.p_eliminar_compuesto(
    in p_compuesto_uuid uuid
)
language plpgsql as $$

declare
    l_total_registros integer;

begin
    -- Verificación de existencia del compuesto
    select count(*) into l_total_registros from core.compuestos where compuesto_uuid = p_compuesto_uuid;
    if l_total_registros = 0 then
        raise exception 'No existe un compuesto registrado con ese UUID';
    end if;

    -- Eliminación del compuesto
    delete from core.compuestos where compuesto_uuid = p_compuesto_uuid;
end; $$;



-- ****************************************
-- Creación de Vistas de Consulta
-- ****************************************

-- Vista de compuestos con detalles de elementos
create or replace view core.v_info_compuestos as
select
    c.compuesto_uuid,
    c.nombre as compuesto_nombre,
    c.formula_quimica,
    c.masa_molar,
    c.estado_agregacion,
    e.elemento_uuid,
    e.nombre as elemento_nombre,
    e.simbolo,
    e.numero_atomico,
    epc.cantidad_atomos
from
    core.compuestos c
    join core.elementos_por_compuestos epc on c.compuesto_uuid = epc.compuesto_uuid
    join core.elementos e on epc.elemento_uuid = e.elemento_uuid;

comment on view core.v_info_compuestos is 'Vista que muestra cada compuesto con sus elementos y cantidades de átomos';

-- Vista de información de elementos
create or replace view core.v_info_elementos as
select
    elemento_uuid,
    nombre,
    simbolo,
    numero_atomico,
    config_electronica
from core.elementos;

comment on view core.v_info_elementos is 'Vista que muestra la información básica de cada elemento';


-- ****************************************
-- Creación Consultas listar por guid
-- ****************************************

--listar un elemento por guid
select * from core.elementos where elemento_uuid = '[uuid]';

--listar un compuesto por guid
select * from core.v_info_compuestos where compuesto_uuid = '[uuid]';





-- *******************************************
-- Realizar Pruebas sin insercion de data csv
-- *******************************************

-- Insertar un nuevo elemento (Hidrógeno)
call core.p_insertar_elemento(
    p_nombre := 'Hidrógeno',
    p_simbolo := 'H',
    p_numero_atomico := 1,
    p_config_electronica := '1s1');

call core.p_insertar_elemento(
    p_nombre := 'Fósforo',
    p_simbolo := 'fo',
    p_numero_atomico := 15,
    p_config_electronica := '1s2 2s2 2p6 3s2 3p3');

-- Ver todos los elementos y compuestos
select * from core.elementos;
select * from core.compuestos;

-- Insertar un nuevo compuesto (Agua - H2O)
call core.p_insertar_compuesto(
    p_nombre := 'Agua',
    p_formula_quimica := 'H2O',
    p_masa_molar := 18.015,
    p_estado_agregacion := 'Líquido');

-- Actualizar el nombre y configuración electrónica de un elemento usando su UUID
call core.p_actualizar_elemento(
    p_elemento_uuid := '8a4e5691-4508-471c-abaa-4061dfbe3a40',
    p_nombre := 'Hidrógeno Modificado',
    p_simbolo := 'H',
    p_numero_atomico := 1,
    p_config_electronica := '1s2');

-- Actualizar la fórmula química y masa molar de un compuesto usando su UUID
call core.p_actualizar_compuesto(
    p_compuesto_uuid := '91463ce4-913c-46ee-ad63-fe823ba61573',
    p_nombre := 'Agua Modificada',
    p_formula_quimica := 'H2O2',
    p_masa_molar := 36.030,
    p_estado_agregacion := 'Líquido');

-- Eliminar un elemento (Hidrógeno) por su UUID
call core.p_eliminar_elemento(p_elemento_uuid := '8a4e5691-4508-471c-abaa-4061dfbe3a40');

-- Eliminar un compuesto (Agua) por su UUID
call core.p_eliminar_compuesto(p_compuesto_uuid := '91463ce4-913c-46ee-ad63-fe823ba61573');

-- Ver todos los elementos
select * from core.v_info_elementos;

-- insertamos manualmente la cantidad de atomos "validar que si corresponda el id agua e hidrogeno" 
INSERT INTO core.elementos_por_compuestos (elemento_uuid,compuesto_uuid, cantidad_atomos)
VALUES ('57f59c7d-9bb6-41de-8510-7ab9a94e8e21', '4a55e56f-54b5-447b-b558-59b15d560184', 2);


--ver la atabla elementos por compuesto
select * from core.elementos_por_compuestos;

--Ver todos los compuestos con sus elementos
select * from core.v_info_compuestos;


