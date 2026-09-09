CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;
DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260829180817_InitialCreate') THEN

    ALTER DATABASE CHARACTER SET utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260829180817_InitialCreate') THEN

    CREATE TABLE `Categorias` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Nome` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `Descricao` varchar(300) CHARACTER SET utf8mb4 NULL,
        `Status` int NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        CONSTRAINT `PK_Categorias` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260829180817_InitialCreate') THEN

    CREATE TABLE `Clientes` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Nome` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
        `Email` varchar(150) CHARACTER SET utf8mb4 NULL,
        `Telefone` varchar(30) CHARACTER SET utf8mb4 NULL,
        `Endereco` varchar(300) CHARACTER SET utf8mb4 NULL,
        `Status` int NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        CONSTRAINT `PK_Clientes` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260829180817_InitialCreate') THEN

    CREATE TABLE `Lojas` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `Nome` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
        `Foto` varchar(500) CHARACTER SET utf8mb4 NULL,
        `Email` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
        `Senha` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `Cnpj` varchar(20) CHARACTER SET utf8mb4 NULL,
        `CorPrimaria` varchar(20) CHARACTER SET utf8mb4 NULL,
        `CorSecundaria` varchar(20) CHARACTER SET utf8mb4 NULL,
        `Status` int NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        CONSTRAINT `PK_Lojas` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260829180817_InitialCreate') THEN

    CREATE TABLE `Produtos` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `CategoriaId` int NOT NULL,
        `Foto` varchar(500) CHARACTER SET utf8mb4 NULL,
        `Nome` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
        `Descricao` varchar(500) CHARACTER SET utf8mb4 NULL,
        `Tamanho` varchar(20) CHARACTER SET utf8mb4 NULL,
        `Cor` varchar(50) CHARACTER SET utf8mb4 NULL,
        `ValorCompra` decimal(10,2) NOT NULL,
        `ValorVenda` decimal(10,2) NOT NULL,
        `QuantidadeEstoque` int NOT NULL,
        `EstoqueMinimo` int NOT NULL,
        `Status` int NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        CONSTRAINT `PK_Produtos` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Produtos_Categorias_CategoriaId` FOREIGN KEY (`CategoriaId`) REFERENCES `Categorias` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260829180817_InitialCreate') THEN

    CREATE TABLE `Vendas` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `ClienteId` int NULL,
        `DataVenda` datetime(6) NOT NULL,
        `FormaPagamento` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `ValorTotal` decimal(10,2) NOT NULL,
        `StatusVenda` int NOT NULL,
        `Status` int NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        CONSTRAINT `PK_Vendas` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Vendas_Clientes_ClienteId` FOREIGN KEY (`ClienteId`) REFERENCES `Clientes` (`Id`) ON DELETE SET NULL
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260829180817_InitialCreate') THEN

    CREATE TABLE `MovimentacoesEstoque` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `ProdutoId` int NOT NULL,
        `Tipo` int NOT NULL,
        `Quantidade` int NOT NULL,
        `Motivo` varchar(255) CHARACTER SET utf8mb4 NULL,
        `DataMovimentacao` datetime(6) NOT NULL,
        `Status` int NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        CONSTRAINT `PK_MovimentacoesEstoque` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_MovimentacoesEstoque_Produtos_ProdutoId` FOREIGN KEY (`ProdutoId`) REFERENCES `Produtos` (`Id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260829180817_InitialCreate') THEN

    CREATE TABLE `ItensVenda` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `VendaId` int NOT NULL,
        `ProdutoId` int NOT NULL,
        `Quantidade` int NOT NULL,
        `ValorUnitario` decimal(10,2) NOT NULL,
        `Subtotal` decimal(10,2) NOT NULL,
        `Status` int NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        CONSTRAINT `PK_ItensVenda` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_ItensVenda_Produtos_ProdutoId` FOREIGN KEY (`ProdutoId`) REFERENCES `Produtos` (`Id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_ItensVenda_Vendas_VendaId` FOREIGN KEY (`VendaId`) REFERENCES `Vendas` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260829180817_InitialCreate') THEN

    CREATE INDEX `IX_ItensVenda_ProdutoId` ON `ItensVenda` (`ProdutoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260829180817_InitialCreate') THEN

    CREATE INDEX `IX_ItensVenda_VendaId` ON `ItensVenda` (`VendaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260829180817_InitialCreate') THEN

    CREATE INDEX `IX_MovimentacoesEstoque_ProdutoId` ON `MovimentacoesEstoque` (`ProdutoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260829180817_InitialCreate') THEN

    CREATE INDEX `IX_Produtos_CategoriaId` ON `Produtos` (`CategoriaId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260829180817_InitialCreate') THEN

    CREATE INDEX `IX_Vendas_ClienteId` ON `Vendas` (`ClienteId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260829180817_InitialCreate') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260829180817_InitialCreate', '9.0.2');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260901233740_AddProdutoVariacoes') THEN

    ALTER TABLE `Produtos` ADD `Marca` varchar(100) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260901233740_AddProdutoVariacoes') THEN

    ALTER TABLE `MovimentacoesEstoque` ADD `VariacaoProdutoId` int NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260901233740_AddProdutoVariacoes') THEN

    ALTER TABLE `ItensVenda` ADD `VariacaoProdutoId` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260901233740_AddProdutoVariacoes') THEN

    CREATE TABLE `VariacoesProduto` (
        `Id` int NOT NULL AUTO_INCREMENT,
        `ProdutoId` int NOT NULL,
        `Tamanho` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `Cor` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `QuantidadeEstoque` int NOT NULL,
        `Status` int NOT NULL,
        `CreatedAt` datetime(6) NOT NULL,
        `UpdatedAt` datetime(6) NULL,
        CONSTRAINT `PK_VariacoesProduto` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_VariacoesProduto_Produtos_ProdutoId` FOREIGN KEY (`ProdutoId`) REFERENCES `Produtos` (`Id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260901233740_AddProdutoVariacoes') THEN


                    INSERT INTO VariacoesProduto (ProdutoId, Tamanho, Cor, QuantidadeEstoque, Status, CreatedAt, UpdatedAt)
                    SELECT Id, COALESCE(Tamanho, ''), COALESCE(Cor, ''), QuantidadeEstoque, Status, CreatedAt, UpdatedAt
                    FROM Produtos
                    WHERE Id NOT IN (SELECT DISTINCT ProdutoId FROM VariacoesProduto);
                

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260901233740_AddProdutoVariacoes') THEN


                    UPDATE ItensVenda iv
                    JOIN VariacoesProduto vp ON vp.ProdutoId = iv.ProdutoId
                    SET iv.VariacaoProdutoId = vp.Id
                    WHERE iv.VariacaoProdutoId = 0 OR iv.VariacaoProdutoId IS NULL;
                

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260901233740_AddProdutoVariacoes') THEN


                    UPDATE MovimentacoesEstoque me
                    JOIN VariacoesProduto vp ON vp.ProdutoId = me.ProdutoId
                    SET me.VariacaoProdutoId = vp.Id
                    WHERE me.VariacaoProdutoId IS NULL;
                

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260901233740_AddProdutoVariacoes') THEN

    ALTER TABLE `Produtos` DROP COLUMN `Cor`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260901233740_AddProdutoVariacoes') THEN

    ALTER TABLE `Produtos` DROP COLUMN `QuantidadeEstoque`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260901233740_AddProdutoVariacoes') THEN

    ALTER TABLE `Produtos` DROP COLUMN `Tamanho`;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260901233740_AddProdutoVariacoes') THEN

    CREATE INDEX `IX_MovimentacoesEstoque_VariacaoProdutoId` ON `MovimentacoesEstoque` (`VariacaoProdutoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260901233740_AddProdutoVariacoes') THEN

    CREATE INDEX `IX_ItensVenda_VariacaoProdutoId` ON `ItensVenda` (`VariacaoProdutoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260901233740_AddProdutoVariacoes') THEN

    CREATE UNIQUE INDEX `IX_VariacoesProduto_ProdutoId_Tamanho_Cor` ON `VariacoesProduto` (`ProdutoId`, `Tamanho`, `Cor`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260901233740_AddProdutoVariacoes') THEN

    ALTER TABLE `ItensVenda` ADD CONSTRAINT `FK_ItensVenda_VariacoesProduto_VariacaoProdutoId` FOREIGN KEY (`VariacaoProdutoId`) REFERENCES `VariacoesProduto` (`Id`) ON DELETE RESTRICT;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260901233740_AddProdutoVariacoes') THEN

    ALTER TABLE `MovimentacoesEstoque` ADD CONSTRAINT `FK_MovimentacoesEstoque_VariacoesProduto_VariacaoProdutoId` FOREIGN KEY (`VariacaoProdutoId`) REFERENCES `VariacoesProduto` (`Id`) ON DELETE RESTRICT;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260901233740_AddProdutoVariacoes') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260901233740_AddProdutoVariacoes', '9.0.2');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260902133551_FixStockHardeningAndVariations') THEN

    ALTER TABLE `MovimentacoesEstoque` MODIFY COLUMN `VariacaoProdutoId` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260902133551_FixStockHardeningAndVariations') THEN

    ALTER TABLE `VariacoesProduto` ADD CONSTRAINT `CK_VariacoesProduto_QuantidadeEstoque` CHECK (`QuantidadeEstoque` >= 0);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260902133551_FixStockHardeningAndVariations') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260902133551_FixStockHardeningAndVariations', '9.0.2');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260903151717_AddApelidoToCliente') THEN

    ALTER TABLE `Clientes` ADD `Apelido` varchar(100) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260903151717_AddApelidoToCliente') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260903151717_AddApelidoToCliente', '9.0.2');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;


-- Codigo de barras por variacao, usado no PDV por bipagem e na impressao de etiquetas.
ALTER TABLE `VariacoesProduto`
  ADD COLUMN IF NOT EXISTS `CodigoBarras` varchar(80) CHARACTER SET utf8mb4 NULL;

CREATE UNIQUE INDEX IF NOT EXISTS `IX_VariacoesProduto_CodigoBarras`
  ON `VariacoesProduto` (`CodigoBarras`);
