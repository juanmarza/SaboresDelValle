-- =============================================================
-- SaboresDelValle - Esquema de base de datos
-- MySQL 8.0+
-- Alcance: exactamente 3 tablas (producto, plato, pedido).
-- Precisión: todo importe o cantidad decimal usa 2 decimales
-- (PrecisionPolicy en Domain; DECIMAL(n,2) como segunda barrera).
-- Base legacy restaurantedb: solo referencia, no se modifica.
-- =============================================================

CREATE DATABASE IF NOT EXISTS saboresdelvalle
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_0900_ai_ci;

USE saboresdelvalle;

-- -------------------------------------------------------------
-- producto: catálogo de materia prima / insumos.
-- -------------------------------------------------------------
CREATE TABLE producto (
    IdProducto    INT            NOT NULL AUTO_INCREMENT,
    Nombre        VARCHAR(100)   NOT NULL,
    Tipo          VARCHAR(20)    CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    UnidadMedida  VARCHAR(10)    CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    StockMinimo   DECIMAL(12,2)  NOT NULL DEFAULT 0.00,
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

-- -------------------------------------------------------------
-- plato: carta del restaurante.
-- -------------------------------------------------------------
CREATE TABLE plato (
    IdPlato     INT            NOT NULL AUTO_INCREMENT,
    Nombre      VARCHAR(100)   NOT NULL,
    Precio      DECIMAL(10,2)  NOT NULL,
    Tipo        VARCHAR(20)    CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
    Disponible  TINYINT(1)     NOT NULL DEFAULT 1,

    CONSTRAINT PK_Plato PRIMARY KEY (IdPlato),
    CONSTRAINT UQ_Plato_Nombre UNIQUE (Nombre),

    CONSTRAINT CK_Plato_Nombre_NoVacio
        CHECK (CHAR_LENGTH(TRIM(Nombre)) > 0),
    CONSTRAINT CK_Plato_Precio_Positivo
        CHECK (Precio > 0),
    CONSTRAINT CK_Plato_Tipo
        CHECK (Tipo IN ('SOPA', 'SEGUNDO', 'EXTRA', 'BEBIDA')),
    CONSTRAINT CK_Plato_Disponible
        CHECK (Disponible IN (0, 1))
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_0900_ai_ci;

-- -------------------------------------------------------------
-- pedido: un plato pedido por una mesa.
-- MetodoPago solo se registra cuando el pedido está PAGADO.
-- -------------------------------------------------------------
CREATE TABLE pedido (
    IdPedido        INT            NOT NULL AUTO_INCREMENT,
    IdPlato         INT            NOT NULL,
    NumeroMesa      INT            NOT NULL,
    Cantidad        INT            NOT NULL,
    PrecioUnitario  DECIMAL(10,2)  NOT NULL,
    Estado          VARCHAR(20)    CHARACTER SET ascii COLLATE ascii_bin NOT NULL DEFAULT 'PENDIENTE',
    MetodoPago      VARCHAR(20)    CHARACTER SET ascii COLLATE ascii_bin NULL,
    Propina         DECIMAL(10,2)  NOT NULL DEFAULT 0.00,
    Descuento       DECIMAL(10,2)  NOT NULL DEFAULT 0.00,
    FechaHora       DATETIME       NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT PK_Pedido PRIMARY KEY (IdPedido),
    CONSTRAINT FK_Pedido_Plato FOREIGN KEY (IdPlato)
        REFERENCES plato (IdPlato)
        ON UPDATE RESTRICT
        ON DELETE RESTRICT,

    CONSTRAINT CK_Pedido_NumeroMesa_Positivo
        CHECK (NumeroMesa > 0),
    CONSTRAINT CK_Pedido_Cantidad_Positiva
        CHECK (Cantidad > 0),
    CONSTRAINT CK_Pedido_PrecioUnitario_Positivo
        CHECK (PrecioUnitario > 0),
    CONSTRAINT CK_Pedido_Estado
        CHECK (Estado IN ('PENDIENTE', 'EN_PREPARACION', 'LISTO', 'PAGADO')),
    CONSTRAINT CK_Pedido_MetodoPago
        CHECK (MetodoPago IS NULL OR MetodoPago IN ('EFECTIVO', 'QR')),
    CONSTRAINT CK_Pedido_MetodoPago_SegunEstado
        CHECK ((Estado = 'PAGADO') = (MetodoPago IS NOT NULL)),
    CONSTRAINT CK_Pedido_Propina_NoNegativa
        CHECK (Propina >= 0),
    CONSTRAINT CK_Pedido_Descuento_NoNegativo
        CHECK (Descuento >= 0)
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_0900_ai_ci;

CREATE INDEX IX_Pedido_Mesa_Estado ON pedido (NumeroMesa, Estado);
