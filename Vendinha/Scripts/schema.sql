PRAGMA foreign_keys = ON;

CREATE TABLE Clientes (
    Id             INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    NomeCompleto   TEXT    NOT NULL,
    CPF            TEXT    NOT NULL CHECK (length(CPF) = 11),
    DataNascimento TEXT    NOT NULL,
    Email          TEXT    NULL
);

CREATE TABLE Dividas (
    Id            INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    Valor         TEXT    NOT NULL,
    Paga          INTEGER NOT NULL DEFAULT 0,
    DataCriacao   TEXT    NOT NULL,
    DataPagamento TEXT    NULL,
    ClienteId     INTEGER NOT NULL,
    CONSTRAINT FK_Dividas_Clientes_ClienteId
        FOREIGN KEY (ClienteId) REFERENCES Clientes (Id) ON DELETE CASCADE
);

CREATE UNIQUE INDEX IX_Clientes_CPF ON Clientes (CPF);

CREATE UNIQUE INDEX IX_Dividas_ClienteId ON Dividas (ClienteId) WHERE Paga = 0;
