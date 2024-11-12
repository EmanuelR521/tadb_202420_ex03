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

-- privilegios adicionales para poder que la api funcione
grant select, insert, update, delete on table core.compuestos TO quimico_usr;
grant select, insert, update, delete on table core.elementos TO quimico_usr;
grant select, insert, update, delete on table core.elementos_por_compuestos TO quimico_usr;
grant select on core.v_info_compuestos to quimico_usr;
grant select on core.v_info_elementos to quimico_usr;

-- Activar la extensión que permite el uso de UUID
create extension if not exists "uuid-ossp";

-- ****************************************
-- Creación de Tablas Principales
-- ****************************************

create table core.elementos (
    id_uuid uuid default gen_random_uuid() primary key,
    nombre varchar(50) not null,
    simbolo varchar(5) not null,
    numero_atomico integer not null,
    config_electronica varchar(50) not null,
    constraint nombre_simbolo_uk unique (nombre, simbolo)
);

comment on table core.elementos is 'Tabla que almacena los elementos químicos';

create table core.compuestos (
    id_uuid uuid default gen_random_uuid() primary key,
    nombre varchar(30) not null,
    formula_quimica varchar(30) not null,
    masa_molar numeric(6,3) not null,
    estado_agregacion varchar(30) not null,
    constraint nombre_formula_quimica_uk unique (nombre, formula_quimica)
);

comment on table core.compuestos is 'Tabla que almacena los compuestos';

create table core.elementos_por_compuestos (
    elemento_uuid uuid not null references core.elementos(id_uuid),
    compuesto_uuid uuid not null references core.compuestos(id_uuid),
    cantidad_atomos integer not null,
    primary key (elemento_uuid, compuesto_uuid)
);

comment on table core.elementos_por_compuestos is 'Relación de cantidad de átomos de cada elemento en un compuesto';

-- ****************************************
-- Creación de Vistas de Consulta
-- ****************************************

-- Listar todos los elementos
create or replace view core.v_info_elementos as
select id_uuid, nombre, simbolo, numero_atomico, config_electronica
from core.elementos;

comment on view core.v_info_elementos is 'Vista que muestra la información básica de cada elemento';

-- Listar todos los compuestos
create or replace view core.v_info_compuestos as
select
    c.id_uuid as compuesto_uuid,
    c.nombre as compuesto_nombre,
    c.formula_quimica,
    c.masa_molar,
    c.estado_agregacion,
    e.id_uuid as elemento_uuid,
    e.nombre as elemento_nombre,
    e.simbolo,
    e.numero_atomico,
    epc.cantidad_atomos
from
    core.compuestos c
    join core.elementos_por_compuestos epc on c.id_uuid = epc.compuesto_uuid
    join core.elementos e on epc.elemento_uuid = e.id_uuid;

comment on view core.v_info_compuestos is 'Vista que muestra cada compuesto con sus elementos y cantidades de átomos';

-- ****************************************
-- Creación de Procedimientos y Funciones para CRUD
-- ****************************************

CREATE OR REPLACE PROCEDURE core.p_obtener_elemento_por_guid(
    p_id_uuid UUID
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_nombre VARCHAR;
    v_simbolo VARCHAR;
    v_numero_atomico INTEGER;
    v_config_electronica VARCHAR;
BEGIN
    -- Obtener el elemento por UUID
    SELECT nombre, simbolo, numero_atomico, config_electronica
    INTO v_nombre, v_simbolo, v_numero_atomico, v_config_electronica
    FROM core.elementos
    WHERE id_uuid = p_id_uuid;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'Elemento no encontrado con el GUID: %', p_id_uuid;
    END IF;


    RAISE NOTICE 'Elemento UUID: %, Nombre: %, Símbolo: %, Número Atómico: %, Config. Electrónica: %',
                 p_id_uuid, v_nombre, v_simbolo, v_numero_atomico, v_config_electronica;
END;
$$;


-- Insertar un nuevo elemento
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
    if p_nombre is null or p_simbolo is null or p_numero_atomico is null or p_config_electronica is null or
       length(p_nombre) = 0 or length(p_simbolo) = 0 or length(p_config_electronica) = 0 then
        raise exception 'El nombre, símbolo, número atómico y configuración electrónica no pueden ser nulos o vacíos';
    end if;

    select count(*) into l_total_registros
    from core.elementos
    where lower(nombre) = lower(p_nombre) or lower(simbolo) = lower(p_simbolo);

    if l_total_registros > 0 then
        raise exception 'Ya existe un elemento con ese nombre o símbolo';
    end if;

    insert into core.elementos (nombre, simbolo, numero_atomico, config_electronica)
    values (initcap(p_nombre), upper(p_simbolo), p_numero_atomico, p_config_electronica);

end; $$;

-- Actualizar un elemento
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
    select count(*) into l_total_registros
    from core.elementos
    where id_uuid = p_elemento_uuid;

    if l_total_registros = 0 then
        raise exception 'No existe un elemento registrado con ese UUID';
    end if;

    if p_nombre is null or p_simbolo is null or p_numero_atomico is null or p_config_electronica is null or
       length(p_nombre) = 0 or length(p_simbolo) = 0 or length(p_config_electronica) = 0 then
        raise exception 'El nombre, símbolo, número atómico y configuración electrónica no pueden ser nulos o vacíos';
    end if;

    select count(*) into l_total_registros
    from core.elementos
    where (lower(nombre) = lower(p_nombre) or lower(simbolo) = lower(p_simbolo))
    and id_uuid != p_elemento_uuid;

    if l_total_registros > 0 then
        raise exception 'Ya existe un elemento con ese nombre o símbolo';
    end if;

    update core.elementos
    set nombre = initcap(p_nombre),
        simbolo = upper(p_simbolo),
        numero_atomico = p_numero_atomico,
        config_electronica = p_config_electronica
    where id_uuid = p_elemento_uuid;

end; $$;

-- Eliminar un elemento
create or replace procedure core.p_eliminar_elemento(
    in p_elemento_uuid uuid
)
language plpgsql as $$

declare
    l_total_registros integer;
begin
    select count(*) into l_total_registros
    from core.elementos
    where id_uuid = p_elemento_uuid;

    if l_total_registros = 0 then
        raise exception 'No existe un elemento registrado con ese UUID';
    end if;

    delete from core.elementos
    where id_uuid = p_elemento_uuid;

end; $$;

-- Listar un compuesto por GUID con sus elementos
CREATE OR REPLACE PROCEDURE core.p_obtener_compuesto_por_guid(
    p_compuesto_uuid UUID
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_compuesto_nombre VARCHAR;
    v_compuesto_formula VARCHAR;
    v_compuesto_peso_molecular REAL;
    v_compuesto_estado VARCHAR;
    v_elemento RECORD;
BEGIN
    -- Obtener el compuesto por UUID
    SELECT nombre, formula_quimica, masa_molar::real, estado_agregacion
    INTO v_compuesto_nombre, v_compuesto_formula, v_compuesto_peso_molecular, v_compuesto_estado
    FROM core.compuestos
    WHERE id_uuid = p_compuesto_uuid;


    IF NOT FOUND THEN
        RAISE EXCEPTION 'Compuesto no encontrado con el GUID: %', p_compuesto_uuid;
    END IF;


    RAISE NOTICE 'Compuesto UUID: %, Nombre: %, Fórmula: %, Peso Molecular: %, Estado: %',
                 p_compuesto_uuid, v_compuesto_nombre, v_compuesto_formula, v_compuesto_peso_molecular, v_compuesto_estado;


    FOR v_elemento IN
        SELECT e.id_uuid, epc.cantidad_atomos
        FROM core.elementos_por_compuestos epc
        JOIN core.elementos e ON e.id_uuid = epc.elemento_uuid
        WHERE epc.compuesto_uuid = p_compuesto_uuid
    LOOP
        -- Mostrar los elementos
        RAISE NOTICE 'Elemento UUID: %, Cantidad de átomos: %', v_elemento.id_uuid, v_elemento.cantidad_atomos;
    END LOOP;
END;
$$;


--actualizar compuesto
CREATE OR REPLACE PROCEDURE core.p_actualizar_compuesto(
    p_compuesto_uuid UUID,
    p_nombre_compuesto VARCHAR,
    p_formula_compuesto VARCHAR,
    p_peso_molecular REAL,
    p_estado_compuesto VARCHAR,
    p_elementos JSON
) LANGUAGE plpgsql AS $$
DECLARE
    v_elemento JSON;
    v_elemento_uuid UUID;
    v_cantidad_atomos INT;
BEGIN

    UPDATE core.compuestos
    SET nombre = initcap(p_nombre_compuesto),
        formula_quimica = p_formula_compuesto,
        masa_molar = p_peso_molecular,
        estado_agregacion = initcap(p_estado_compuesto)
    WHERE id_uuid = p_compuesto_uuid;


    FOR v_elemento IN
        SELECT * FROM json_array_elements(p_elementos)
    LOOP

        v_elemento_uuid := (v_elemento->>'elemento_uuid')::UUID;
        v_cantidad_atomos := (v_elemento->>'cantidad_atomos')::INT;


        RAISE NOTICE 'Elemento UUID: %, Cantidad de átomos: %', v_elemento_uuid, v_cantidad_atomos;


        UPDATE core.elementos_por_compuestos
        SET cantidad_atomos = v_cantidad_atomos
        WHERE compuesto_uuid = p_compuesto_uuid AND elemento_uuid = v_elemento_uuid;
    END LOOP;
END;
$$;

-- Insertar un nuevo compuesto incluyendo elementos
CREATE OR REPLACE PROCEDURE core.p_insertar_compuesto(IN p_nombre text, IN p_formula_quimica text, IN p_masa_molar numeric, IN p_estado_agregacion text, IN p_elementos text)
 LANGUAGE plpgsql
AS $procedure$
declare
    v_compuesto_uuid uuid;
    v_elemento json;
    v_elemento_uuid uuid;
    v_cantidad_atomos int;
begin
    if p_nombre is null or p_formula_quimica is null then
        raise exception 'El nombre y la fórmula química no pueden ser nulos';
    end if;

    insert into core.compuestos (nombre, formula_quimica, masa_molar, estado_agregacion)
    values (initcap(p_nombre), p_formula_quimica, p_masa_molar, initcap(p_estado_agregacion))
    returning id_uuid into v_compuesto_uuid;

    for v_elemento in select * from json_array_elements(p_elementos::json) loop
        v_elemento_uuid := (v_elemento->>'uuid')::uuid;
        v_cantidad_atomos := (v_elemento->>'cantidad_atomos')::int;

        insert into core.elementos_por_compuestos (elemento_uuid, compuesto_uuid, cantidad_atomos)
        values (v_elemento_uuid, v_compuesto_uuid, v_cantidad_atomos);
    end loop;
end;
$procedure$
;



-- Eliminar un compuesto
create or replace procedure core.p_eliminar_compuesto(
    in p_compuesto_uuid uuid
)
language plpgsql as $$

declare
    l_total_registros integer;
begin
    select count(*) into l_total_registros
    from core.compuestos
    where id_uuid = p_compuesto_uuid;

    if l_total_registros = 0 then
        raise exception 'No existe un compuesto registrado con ese UUID';
    end if;

    delete from core.elementos_por_compuestos
    WHERE compuesto_uuid = p_compuesto_uuid;

    delete from core.compuestos
    where id_uuid = p_compuesto_uuid;

end; $$;


--***************
--zona de pruebas
--***************

call core.p_insertar_elemento('Fosforo', 'fo', 15, '1s1 1s3');
select * from core.v_info_elementos;
call core.p_obtener_elemento_por_guid('123e4567-e89b-12d3-a456-426614174000');
call core.p_actualizar_elemento('49c9e626-61af-476f-8e21-e39c2aeb26aa', 'Hidrógeno Actualizado', 'H', 1, '1s1 Actualizado');
call core.p_eliminar_elemento('49c9e626-61af-476f-8e21-e39c2aeb26aa');

-- Insertar Hidrógeno
INSERT INTO core.elementos (id_uuid, nombre, simbolo, numero_atomico, config_electronica)
VALUES ('123e4567-e89b-12d3-a456-426614174000', 'Hidrógeno', 'H', 1, '1s1');

-- Insertar Oxígeno
INSERT INTO core.elementos (id_uuid, nombre, simbolo, numero_atomico, config_electronica)
VALUES ('987e4567-e89b-12d3-a456-426614174111', 'Oxígeno', 'O', 8, '1s2 2s2 2p4');


call core.p_insertar_compuesto(
    'Agua',
    'H2O',
    18.015,
    'Líquido',
    '[{"elemento_uuid": "123e4567-e89b-12d3-a456-426614174000", "cantidad_atomos": 2}, {"elemento_uuid": "987e4567-e89b-12d3-a456-426614174111", "cantidad_atomos": 1}]'
);

select * from core.compuestos;
select * from core.elementos;
select * from core.v_info_compuestos;
select * from core.v_info_elementos;

CALL core.p_actualizar_compuesto(
    '33219da0-c9c1-49f5-8bfc-f407e36d233f',
    'Agua Modificada',
    'H2O',
    18.015,
    'Líquido Modificado',
    '[{"elemento_uuid": "123e4567-e89b-12d3-a456-426614174000", "cantidad_atomos": 2},
      {"elemento_uuid": "987e4567-e89b-12d3-a456-426614174111", "cantidad_atomos": 1}]'::json
);

call core.p_eliminar_compuesto('33219da0-c9c1-49f5-8bfc-f407e36d233f');
call core.p_obtener_compuesto_por_guid('1d145cbd-d06d-4cf0-8f35-214caf413cd2');

select * from core.v_info_compuestos;