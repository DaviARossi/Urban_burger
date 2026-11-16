


CREATE DATABASE DB_Delivery CHARACTER SET utf8mb4;
USE DB_Delivery;
SET NAMES utf8mb4;


CREATE TABLE clientes (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nome VARCHAR(80) NOT NULL,
    email VARCHAR(100) NOT NULL UNIQUE,
    cpf VARCHAR(14) NOT NULL UNIQUE,
    telefone VARCHAR(15) NOT NULL,
    endereco VARCHAR(200) NOT NULL,
    senha_hash VARCHAR(255) NULL
) ENGINE=InnoDB;

CREATE TABLE categorias (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nome VARCHAR(40) NOT NULL
) ENGINE=InnoDB;

CREATE TABLE produtos (
    id INT AUTO_INCREMENT PRIMARY KEY,
    categoria_id INT NOT NULL,
    nome VARCHAR(80) NOT NULL,
    descricao VARCHAR(255) NOT NULL,
    preco DECIMAL(10,2) NOT NULL,
    imagem VARCHAR(200) NOT NULL,
    FOREIGN KEY (categoria_id) REFERENCES categorias(id)
) ENGINE=InnoDB;


CREATE TABLE pedidos (
    id INT AUTO_INCREMENT PRIMARY KEY,
    cliente_id INT NOT NULL,
    codigo_origem VARCHAR(64) NULL UNIQUE,
    data_pedido DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    status VARCHAR(30) NOT NULL DEFAULT 'Em preparação',
    endereco_entrega VARCHAR(200) NOT NULL,
    valor_total DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (cliente_id) REFERENCES clientes(id)
) ENGINE=InnoDB;


CREATE TABLE itens_pedido (
    id INT AUTO_INCREMENT PRIMARY KEY,
    pedido_id INT NOT NULL,
    produto_id INT NOT NULL,
    quantidade INT NOT NULL,
    preco_unitario DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (pedido_id) REFERENCES pedidos(id),
    FOREIGN KEY (produto_id) REFERENCES produtos(id)
) ENGINE=InnoDB;


INSERT INTO categorias (id, nome) VALUES
    (1, 'Hambúrgueres'),
    (2, 'Acompanhamentos'),
    (3, 'Bebidas'),
    (4, 'Combos');


INSERT INTO produtos (id, categoria_id, nome, descricao, preco, imagem) VALUES
    (1, 1, 'Urban Classic', 'Brioche, burger de 160 g, cheddar, alface, tomate e molho da casa.', 28.90, 'img/burger-classic.jpg'),
    (2, 1, 'Urban Bacon', 'Burger de 160 g, cheddar, bacon crocante e maionese da casa.', 33.90, 'img/burger-bacon.jpg'),
    (3, 1, 'Urban Duplo', 'Dois burgers de 120 g, queijo em dobro, picles e molho especial.', 38.90, 'img/burger-duplo.jpg'),
    (4, 1, 'Urban Veggie', 'Burger de grão-de-bico, queijo, folhas, tomate e molho de ervas.', 30.90, 'img/burger-veggie.jpg'),
    (5, 2, 'Batata da Casa', 'Batatas crocantes com páprica. Porção de 200 g.', 14.90, 'img/batatas.jpg'),
    (6, 2, 'Onion Rings', 'Anéis de cebola empanados. Porção de 180 g.', 18.90, 'img/onion-rings.jpg'),
    (7, 3, 'Refrigerante Lata', 'Refrigerante de cola em lata de 350 ml.', 6.90, 'img/refrigerante.jpg'),
    (8, 3, 'Suco de Laranja', 'Suco de laranja em copo de 300 ml.', 9.90, 'img/suco.jpg'),
    (9, 4, 'Combo Urban', 'Urban Classic, Batata da Casa e refrigerante.', 39.90, 'img/combo.jpg'),
    (10, 4, 'Combo em Dobro', 'Dois Urban Classics, uma batata e dois refrigerantes.', 74.90, 'img/combo.jpg');


SHOW TABLES;

SELECT * FROM categorias;
SELECT * FROM produtos;


SELECT nome, preco FROM produtos WHERE categoria_id = 1;

SELECT pedido_id, produto_id, quantidade, preco_unitario,
       quantidade * preco_unitario AS subtotal
FROM itens_pedido;
CREATE TABLE gerentes (
    id INT PRIMARY KEY,
    email VARCHAR(100) NOT NULL UNIQUE,
    senha_hash VARCHAR(255) NOT NULL
) ENGINE=InnoDB;
CREATE TABLE importacoes (
    codigo VARCHAR(64) PRIMARY KEY
) ENGINE=InnoDB;
