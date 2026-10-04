-- =============================================================
-- SaboresDelValle - Esquema de base de datos
-- MySQL 8.0+
-- Base legacy restaurantedb: solo referencia, no se modifica.
-- =============================================================

CREATE DATABASE IF NOT EXISTS saboresdelvalle
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_0900_ai_ci;

USE saboresdelvalle;

-- -------------------------------------------------------------
-- producto: catálogo de materia prima / insumos.
-- Sin stock ni costo: se derivarán de lotes y movimientos (PEPS).
-- -------------------------------------------------------------
CREATE TABLE producto (
    IdProducto    INT            NOT NULL AUTO_INCREMENT,
    Nombre        VARCHAR(100)   NOT NULL,
    Tipo          VARCHAR(20)    CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    UnidadMedida  VARCHAR(10)    CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    StockMinimo   DECIMAL(12,3)  NOT NULL DEFAULT 0.000,
    Activo        TINYINT(1)     NOT NULL DEFAULT 1,

    CONSTRAINT PK_Producto PRIMARY KEY (IdProducto),
    CONSTRAINT UQ_Producto_Nombre UNIQUE (Nombre),

    CONSTRAINT CK_Producto_Nombre_NoVacio
        CHECK (CHAR_LENGTH(TRIM(Nombre)) > 0),
    CONSTRAINT CK_Producto_Tipo
        CHECK (Tipo IN ('PERECEDERO', 'NO_PERECEDERO')),
    CONSTRAINT CK_Producto_UnidadMedida
        CHECK (UnidadMedida IN ('kg', 'l', 'unidad')),
    CONSTRAINT CK_Producto_StockMinimo_NoNegativo
        CHECK (StockMinimo >= 0),
    CONSTRAINT CK_Producto_StockMinimo_EnteroSiUnidad
        CHECK (UnidadMedida <> 'unidad' OR StockMinimo = FLOOR(StockMinimo)),
    CONSTRAINT CK_Producto_Activo
        CHECK (Activo IN (0, 1))
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_0900_ai_ci;
