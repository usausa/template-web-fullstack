CREATE TABLE IF NOT EXISTS Data (
    Id         BIGSERIAL  NOT NULL,
    Name       TEXT       NOT NULL,
    Value      INTEGER    NOT NULL,
    Version    INTEGER    NOT NULL,
    CreatedAt  TIMESTAMP  NOT NULL,
    PRIMARY KEY (Id),
    UNIQUE (Name)
);
